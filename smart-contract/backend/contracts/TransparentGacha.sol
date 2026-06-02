// SPDX-License-Identifier: MIT
pragma solidity ^0.8.19;

// 체인링크 VRF V2.5 (최신 버전) 라이브러리 가져오기
import {VRFConsumerBaseV2Plus} from "@chainlink/contracts/src/v0.8/vrf/dev/VRFConsumerBaseV2Plus.sol";
import {VRFV2PlusClient} from "@chainlink/contracts/src/v0.8/vrf/dev/libraries/VRFV2PlusClient.sol";

contract TransparentGacha is VRFConsumerBaseV2Plus {
    // --- [1. 상태 변수 세팅] ---
    uint256 public s_subscriptionId;
    bytes32 public s_keyHash;

    // 아이템 등급 정의
    enum Rarity { COMMON, RARE, EPIC, LEGENDARY }

    // 💡 핵심 1: 릴레이어 대납 시 유저 구분을 위해 [요청ID -> 유저ID] 매핑
    mapping(uint256 => string) public requestToUser;

    // --- [2. 이벤트 세팅 (온체인 기록용 영수증)] ---
    event GachaRequested(uint256 indexed requestId, string userId);
    event GachaResult(uint256 indexed requestId, string userId, uint256 randomNumber, Rarity rarity);

    // 생성자 (네트워크 설정)
    constructor(uint256 subscriptionId, address vrfCoordinator, bytes32 keyHash)
        VRFConsumerBaseV2Plus(vrfCoordinator)
    {
        s_subscriptionId = subscriptionId;
        s_keyHash = keyHash;
    }

    // --- [3. 가챠 요청 함수 (마스터 지갑이 호출)] ---
    // msg.sender는 마스터 지갑이지만, 매개변수로 진짜 플레이어(userId)를 받습니다.
    function rollGacha(string memory userId) external returns (uint256 requestId) {
        // 체인링크에 조작 불가능한 난수 1개를 요청합니다.
        requestId = s_vrfCoordinator.requestRandomWords(
            VRFV2PlusClient.RandomWordsRequest({
                keyHash: s_keyHash,
                subId: s_subscriptionId,
                requestConfirmations: 3,
                callbackGasLimit: 250000,
                numWords: 1, // 1회 뽑기
                extraArgs: VRFV2PlusClient._argsToBytes(VRFV2PlusClient.ExtraArgsV1({nativePayment: false}))
            })
        );

        // 요청 ID와 유저 ID를 연결해 둡니다.
        requestToUser[requestId] = userId;

        emit GachaRequested(requestId, userId);
        return requestId;
    }

    // --- [4. 체인링크 오라클의 난수 응답 및 확률 계산] ---
    function fulfillRandomWords(uint256 requestId, uint256[] calldata randomWords) internal override {
        // 1. 체인링크가 보내준 엄청나게 긴 순수 난수 획득
        uint256 randomNum = randomWords[0];
        
        // 2. 1 ~ 100 사이의 숫자로 가공 (%)
        uint256 chance = (randomNum % 100) + 1;
        
        // 3. 💡 핵심 2: 컨트랙트에 고정된 투명한 확률 테이블
        Rarity result;
        if (chance <= 1) {
            result = Rarity.LEGENDARY; // 1% 확률
        } else if (chance <= 6) {
            result = Rarity.EPIC;      // 5% 확률
        } else if (chance <= 26) {
            result = Rarity.RARE;      // 20% 확률
        } else {
            result = Rarity.COMMON;    // 74% 확률
        }

        // 4. 가챠를 요청했던 원래 유저 ID 불러오기
        string memory userId = requestToUser[requestId];

        // 5. 💡 핵심 3: 최종 결과를 이더스캔(블록체인)에 영구 박제
        emit GachaResult(requestId, userId, randomNum, result);
    }
}