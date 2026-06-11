// SPDX-License-Identifier: MIT
pragma solidity ^0.8.19;

import {VRFConsumerBaseV2Plus} from "@chainlink/contracts/src/v0.8/vrf/dev/VRFConsumerBaseV2Plus.sol";
import {VRFV2PlusClient} from "@chainlink/contracts/src/v0.8/vrf/dev/libraries/VRFV2PlusClient.sol";

// 
// Chainlink VRFConsumerBaseV2Plus를 상속받아 검증 가능한 난수 생성 기능을 사용한다.
contract TransparentGacha is VRFConsumerBaseV2Plus {
    uint256 public s_subscriptionId;
    bytes32 public s_keyHash;
    uint32 callbackGasLimit = 500000;

    mapping(address => uint256) public beginnerPullCount;
    mapping(address => uint256) public midPullCount;

    // 가챠 요청 ID별 데이터를 매핑하여 비동기 콜백 시 식별자로 사용한다.
    mapping(uint256 => uint8) public requestToGachaType;
    mapping(uint256 => address) public requestToSender;
    mapping(uint256 => uint32) public requestToAmount;

    event GachaRolled(address indexed user, uint256 requestId, uint8 gachaType, uint32 amount);
    event GachaResultBatch(address indexed user, uint256 requestId, uint8 gachaType, uint8[] weaponGrades);

    constructor(uint256 subscriptionId, address vrfCoordinator, bytes32 keyHash) 
        VRFConsumerBaseV2Plus(vrfCoordinator) 
    {
        s_subscriptionId = subscriptionId;
        s_keyHash = keyHash;
    }

    // 유저가 가챠를 요청하는 함수
    function rollGacha(address user, uint8 gachaType, uint32 amount) external {
        require(amount == 1 || amount == 10, "Amount must be 1 or 10");

        // 가챠 등급별 잠금 해제 조건을 확인한다. (초급은 무제한, 중급은 100회, 고급은 200회)
        if (gachaType == 1) {
            require(beginnerPullCount[user] >= 100, "Need 100 Beginner pulls first!");
        } else if (gachaType == 2) {
            require(midPullCount[user] >= 100, "Need 100 Mid pulls first!");
        } else {
            require(gachaType == 0, "Invalid Gacha Type!");
        }

        // Chainlink VRF에 난수 생성 요청을 보낸다.
        uint256 requestId = s_vrfCoordinator.requestRandomWords(
            VRFV2PlusClient.RandomWordsRequest({
                keyHash: s_keyHash,
                subId: s_subscriptionId,
                requestConfirmations: 3,
                callbackGasLimit: callbackGasLimit,
                numWords: amount, // 요청한 뽑기 횟수만큼 난수를 생성
                extraArgs: VRFV2PlusClient._argsToBytes(VRFV2PlusClient.ExtraArgsV1({nativePayment: false}))
            })
        );

        // 콜백 호출 시 사용할 데이터를 저장
        requestToGachaType[requestId] = gachaType;
        requestToSender[requestId] = user;
        requestToAmount[requestId] = amount;

        emit GachaRolled(user, requestId, gachaType, amount);
    }

    // Chainlink VRF 노드가 난수 생성을 완료하면 자동으로 호출하는 콜백 함수
    function fulfillRandomWords(uint256 requestId, uint256[] calldata randomWords) internal override {
        uint8 gachaType = requestToGachaType[requestId];
        address user = requestToSender[requestId];
        uint32 amount = requestToAmount[requestId];
        
        uint8[] memory results = new uint8[](amount);

        // 생성된 난수들을 순회하며 아이템 등급을 산출
        for (uint32 i = 0; i < amount; i++) {
            // 1~100까지의 숫자로 변환
            uint256 chance = (randomWords[i] % 100) + 1; 
            
            // 확률 계산 (0~40: 40%, 41~70: 30%, 71~90: 20%, 91~97: 7%, 98~100: 3%)
            uint8 gradeOffset = (chance <= 40) ? 0 : (chance <= 70) ? 1 : (chance <= 90) ? 2 : (chance <= 97) ? 3 : 4;

            // 가챠 타입에 따라 고유한 무기 등급 범위를 설정
            uint8 weaponGrade = (gachaType == 0) ? (1 + gradeOffset) : (gachaType == 1) ? (4 + gradeOffset) : (6 + gradeOffset);
            
            // 누적 뽑기 횟수 카운팅
            if (gachaType == 0) beginnerPullCount[user]++;
            else if (gachaType == 1) midPullCount[user]++;

            results[i] = weaponGrade;
        }

        // 최종 가챠 결과를 이벤트로 발행하여 오프체인(서버)에서 추적 가능하게 함
        emit GachaResultBatch(user, requestId, gachaType, results);
    }
}