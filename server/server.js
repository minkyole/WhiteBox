require('dotenv').config();
const express = require('express');
const cors = require('cors');
const { ethers } = require('ethers');
const fs = require('fs');
const path = require('path');

const app = express();
app.use(cors());
app.use(express.json());

const historyFilePath = path.join(__dirname, 'history.json');
let historyDB = [];

// 서버가 켜질 때 기존 파일이 있으면 읽어오고, 없으면 새로 만듭니다.
if (fs.existsSync(historyFilePath)) {
    const rawData = fs.readFileSync(historyFilePath, 'utf8');
    historyDB = JSON.parse(rawData);
    console.log(`[서버 DB] 기존 가챠 기록 ${historyDB.length}개 불러오기 완료!`);
} else {
    fs.writeFileSync(historyFilePath, JSON.stringify(historyDB, null, 2));
    console.log("[서버 DB] 새로운 history.json 파일을 생성했습니다.");
}

const provider = new ethers.JsonRpcProvider(process.env.RPC_URL);
const masterWallet = new ethers.Wallet(process.env.PRIVATE_KEY, provider);

// 컨트랙트 ABI - rollGacha 함수와 이벤트들만 포함
const contractABI = [
    "function rollGacha(address user, uint8 gachaType, uint32 amount) external",
    "event GachaRolled(address indexed user, uint256 requestId, uint8 gachaType, uint32 amount)",
    "event GachaResultBatch(address indexed user, uint256 requestId, uint8 gachaType, uint8[] weaponGrades)",
    "function beginnerPullCount(address) view returns (uint256)",
    "function midPullCount(address) view returns (uint256)"
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

        const targetEvent = events.find(e => e.args.requestId.toString() === requestId.toString());

        if (targetEvent) {
            const weaponGrades = targetEvent.args.weaponGrades.map(g => Number(g));
            const fulfillTxHash = targetEvent.transactionHash; 
            
            // 🌟 이벤트에서 정보 추출
            const userAddress = targetEvent.args.user; 
            const gachaType = Number(targetEvent.args.gachaType);

            // 🌟 중복 저장이 아니면 DB 배열에 넣고 파일로 저장!
            if (!historyDB.find(h => h.requestId === requestId.toString())) {
                historyDB.push({
                    userAddress: userAddress.toLowerCase(),
                    requestId: requestId.toString(),
                    gachaType: gachaType,
                    weaponGrades: weaponGrades,
                    txHash: fulfillTxHash
                });
                
                // 파일에 예쁘게(들여쓰기 2칸) 덮어쓰기하여 영구 보존
                fs.writeFileSync(historyFilePath, JSON.stringify(historyDB, null, 2));
                console.log(`[서버 DB] ${userAddress}의 기록이 history.json에 영구 저장되었습니다.`);
            }

            console.log(`[서버] 온체인 결과 반환 완료! ID: ${requestId}`);
            
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

// 유저의 가챠 횟수를 조회하는 API
app.get('/api/gacha-counts', async (req, res) => {
    try {
        const userAddress = req.query.address;
        
        // 컨트랙트에서 public 변수 읽기 (가스비 소모 X)
        const beginner = await contract.beginnerPullCount(userAddress);
        const mid = await contract.midPullCount(userAddress);

        res.json({
            success: true,
            // ethers v6 기준 BigInt로 반환되므로 Number나 String으로 변환해서 보냅니다.
            beginnerCount: Number(beginner),
            midCount: Number(mid)
        });
    } catch (error) {
        console.error("횟수 조회 실패:", error);
        res.status(500).json({ success: false, error: error.message });
    }
});


// [유저별 가챠 기록 전체 조회 API]
app.get('/api/history', async (req, res) => {
    try {
        const { address } = req.query;
        if (!address) return res.status(400).json({ success: false, error: "주소가 필요합니다." });

        // 파일에서 읽어온 historyDB 배열에서 내 주소와 일치하는 것만 필터링
        const myHistory = historyDB.filter(h => h.userAddress === address.toLowerCase());

        // 유니티로 전송!
        res.json({ success: true, history: myHistory });

    } catch (error) {
        console.error("[서버] 기록 조회 실패:", error);
        res.status(500).json({ success: false, error: error.message });
    }
});

const PORT = 3000;
app.listen(PORT, () => console.log(`🚀 가스리스 릴레이어 서버 구동 중 (${PORT})`));