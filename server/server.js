require('dotenv').config();
const express = require('express');
const cors = require('cors');
const { ethers } = require('ethers');

const app = express();
app.use(cors());
app.use(express.json());

const provider = new ethers.JsonRpcProvider(process.env.RPC_URL);
const masterWallet = new ethers.Wallet(process.env.PRIVATE_KEY, provider);

// 컨트랙트 ABI - rollGacha 함수와 이벤트들만 포함
const contractABI = [
    "function rollGacha(address user, uint8 gachaType, uint32 amount) external",
    "event GachaRolled(address indexed user, uint256 requestId, uint8 gachaType, uint32 amount)",
    "event GachaResultBatch(address indexed user, uint256 requestId, uint8 gachaType, uint8[] weaponGrades)"
];
const contract = new ethers.Contract(process.env.CONTRACT_ADDRESS, contractABI, masterWallet);

// [가챠 요청 API] - 유니티에서 userAddress, gachaType, amount를 받습니다.
app.post('/api/gacha', async (req, res) => {
    try {
        const { userAddress, gachaType, amount } = req.body; 
        console.log(`[서버] ${userAddress}의 ${amount}연뽑 요청 수신!`);

        // 🌟 컨트랙트 함수 호출 인자 3개로 변경
        const tx = await contract.rollGacha(userAddress, gachaType, amount);
        const receipt = await tx.wait(); 
        
        // GachaRolled 이벤트에서 requestId 추출
        let requestId = "";
        for (const log of receipt.logs) {
            try {
                const parsedLog = contract.interface.parseLog(log);
                if (parsedLog && parsedLog.name === 'GachaRolled') {
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

// [결과 확인 API] - GachaResultBatch 이벤트 기반으로 수정
app.get('/api/gacha-result', async (req, res) => {
    try {
        const { requestId } = req.query;
        if (!requestId) return res.status(400).json({ success: false, error: "requestId 필수" });

        // 🌟 수정 1: 필터 조건 없이 빈 필터를 만듭니다.
        const filter = contract.filters.GachaResultBatch();
        const currentBlock = await provider.getBlockNumber();
        
        // 최근 20블록 안에서 발생한 '모든' 가챠 결과 이벤트를 가져옵니다.
        const events = await contract.queryFilter(filter, currentBlock - 9, currentBlock); 

        // 🌟 수정 2: 가져온 이벤트들 중에서 우리가 찾는 requestId와 일치하는 것만 찾아냅니다.
        const targetEvent = events.find(e => e.args.requestId.toString() === requestId.toString());

        if (targetEvent) {
            // targetEvent에서 배열 형태의 weaponGrades 추출
            const weaponGrades = targetEvent.args.weaponGrades.map(g => Number(g));
            const fulfillTxHash = targetEvent.transactionHash; 

            console.log(`[서버] 온체인 결과 발견! ID: ${requestId} -> 등급들: ${weaponGrades}`);
            
            return res.json({ 
                status: "success", 
                weaponGrades: weaponGrades,
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