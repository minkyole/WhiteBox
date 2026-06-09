// SPDX-License-Identifier: MIT
pragma solidity ^0.8.19;

import {VRFConsumerBaseV2Plus} from "@chainlink/contracts/src/v0.8/vrf/dev/VRFConsumerBaseV2Plus.sol";
import {VRFV2PlusClient} from "@chainlink/contracts/src/v0.8/vrf/dev/libraries/VRFV2PlusClient.sol";

contract TransparentGacha is VRFConsumerBaseV2Plus {
    uint256 public s_subscriptionId;
    bytes32 public s_keyHash;
    uint32 callbackGasLimit = 500000;

    mapping(address => uint256) public beginnerPullCount;
    mapping(address => uint256) public midPullCount;

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

    function rollGacha(address user, uint8 gachaType, uint32 amount) external {
        require(amount == 1 || amount == 10, "Amount must be 1 or 10");

        if (gachaType == 1) {
            require(beginnerPullCount[user] >= 100, "Need 100 Beginner pulls first!");
        } else if (gachaType == 2) {
            require(midPullCount[user] >= 100, "Need 100 Mid pulls first!");
        } else {
            require(gachaType == 0, "Invalid Gacha Type!");
        }

        // 🌟 V2.5 요청 방식: VRFV2PlusClient 사용
        uint256 requestId = s_vrfCoordinator.requestRandomWords(
            VRFV2PlusClient.RandomWordsRequest({
                keyHash: s_keyHash,
                subId: s_subscriptionId,
                requestConfirmations: 3,
                callbackGasLimit: callbackGasLimit,
                numWords: amount,
                extraArgs: VRFV2PlusClient._argsToBytes(VRFV2PlusClient.ExtraArgsV1({nativePayment: false}))
            })
        );

        requestToGachaType[requestId] = gachaType;
        requestToSender[requestId] = user;
        requestToAmount[requestId] = amount;

        emit GachaRolled(user, requestId, gachaType, amount);
    }

    function fulfillRandomWords(uint256 requestId, uint256[] calldata randomWords) internal override {
        uint8 gachaType = requestToGachaType[requestId];
        address user = requestToSender[requestId];
        uint32 amount = requestToAmount[requestId];
        
        uint8[] memory results = new uint8[](amount);

        for (uint32 i = 0; i < amount; i++) {
            uint256 chance = (randomWords[i] % 100) + 1; 
            uint8 gradeOffset = (chance <= 40) ? 0 : (chance <= 70) ? 1 : (chance <= 90) ? 2 : (chance <= 97) ? 3 : 4;

            uint8 weaponGrade = (gachaType == 0) ? (1 + gradeOffset) : (gachaType == 1) ? (4 + gradeOffset) : (6 + gradeOffset);
            
            if (gachaType == 0) beginnerPullCount[user]++;
            else if (gachaType == 1) midPullCount[user]++;

            results[i] = weaponGrade;
        }

        emit GachaResultBatch(user, requestId, gachaType, results);
    }
}