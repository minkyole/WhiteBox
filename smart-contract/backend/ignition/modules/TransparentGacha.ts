import { buildModule } from "@nomicfoundation/hardhat-ignition/modules";

export default buildModule("TransparentGachaModule", (m) => {
  // 체인링크 VRF V2.5 세폴리아(Sepolia) 고정 주소들
  const vrfCoordinator = "0x9DdfaCa8183c41ad55329BdeeD9F6A8d53168B1B";
  const keyHash = "0x787d74caea10b2b357790d5b5247c2f63d1d91572a9846f780606e4d953677ae";

  // ⚠️ 중요: 방금 체인링크 화면에서 복사한 Subscription ID (긴 숫자)를 따옴표 안에 넣어주세요!
  const subscriptionId = "66542399975888666017360326636052430473829468331381051080722436863981852163238";

  // 컨트랙트 배포 세팅
  const gacha = m.contract("TransparentGacha", [
    subscriptionId,
    vrfCoordinator,
    keyHash,
  ]);

  return { gacha };
});