require('dotenv').config();
const express = require('express');
const cors = require('cors');
const { ethers } = require('ethers');

const app = express();
app.use(cors());
app.use(express.json());

const provider = new ethers.JsonRpcProvider(process.env.RPC_URL);
const masterWallet = new ethers.Wallet(process.env.PRIVATE_KEY, provider);

// 1. 이벤트 로그를 읽기 위해 ABI에 Event 내용 추가
const contractABI = [
    "function rollGacha(string memory userId) external returns (uint256)",
    "event GachaRequested(uint256 indexed requestId, string userId)",
    "event GachaResult(uint256 indexed requestId, string userId, uint256 randomNumber, uint8 rarity)"
];
const contract = new ethers.Contract(process.env.CONTRACT_ADDRESS, contractABI, masterWallet);

// [가챠 요청 API] - 영수증과 함께 체인링크 요청ID(requestId)를 함께 반환합니다.
app.post('/api/gacha', async (req, res) => {
    try {
        const { userId } = req.body; 
        console.log(`[서버] ${userId}의 가챠 요청 수신! 트랜잭션 발송 중...`);

        const tx = await contract.rollGacha(userId.toString());
        const receipt = await tx.wait(); // 블록에 기록될 때까지 잠시 대기
        
        // 트랜잭션 영수증 로그에서 체인링크가 발급한 requestId 추출
        let requestId = "";
        for (const log of receipt.logs) {
            try {
                const parsedLog = contract.interface.parseLog(log);
                if (parsedLog && parsedLog.name === 'GachaRequested') {
                    requestId = parsedLog.args.requestId.toString();
                    break;
                }
            } catch (e) {}
        }

        console.log(`[서버] 요청 완료! RequestID: ${requestId} | TxHash: ${tx.hash}`);
        res.json({ success: true, txHash: tx.hash, requestId: requestId });

    } catch (error) {
        console.error("[서버] 가챠 실패:", error);
        res.status(500).json({ success: false, error: error.message });
    }
});

// [최종본] 결과 확인 API - 배달 영수증 해시(fulfillTxHash) 추가 리턴
app.get('/api/gacha-result', async (req, res) => {
    try {
        const { requestId } = req.query;
        if (!requestId) return res.status(400).json({ success: false, error: "requestId 필수" });

        const filter = contract.filters.GachaResult(BigInt(requestId));
        const currentBlock = await provider.getBlockNumber();
        const events = await contract.queryFilter(filter, currentBlock - 9, currentBlock);

        if (events.length > 0) {
            const rarity = Number(events[0].args.rarity); 
            
            // 🌟 [핵심 추가] 체인링크가 난수를 배달한 진짜 트랜잭션의 해시(영수증) 추출!
            const fulfillTxHash = events[0].transactionHash; 

            console.log(`[서버] 온체인 결과 발견! RequestID: ${requestId} -> 등급: ${rarity}`);
            
            // 유니티로 등급과 배달 해시를 함께 던져줍니다.
            return res.json({ 
                status: "success", 
                rarity: rarity, 
                fulfillTxHash: fulfillTxHash 
            });
        } else {
            return res.json({ status: "pending" });
        }
    } catch (error) {
        console.error("[서버] 결과 조회 중 에러 발생:", error);
        res.status(500).json({ success: false, error: error.message });
    }
});

const PORT = 3000;
app.listen(PORT, () => console.log(`🚀 가스리스 릴레이어 서버 구동 중 (${PORT})`));