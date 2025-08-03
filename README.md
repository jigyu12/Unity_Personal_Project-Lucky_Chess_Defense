#  🍀Lucky Chess Defense

🛠️ **개발 도구**
 <img src="https://img.shields.io/badge/C%23-80247B?style=flat-square&logo=csharp&logoColor=white"/> <img src="https://img.shields.io/badge/Unity-000000?style=flat-square&logo=unity&logoColor=white"/>

📅 **개발 기간**
 25.01.21 ~ 25.02.14 (3주)

🧑‍💻 **개발진**
 <img src="https://img.shields.io/badge/민지규-80247B?style=flat-square&logo=&logoColor=white"/> <img src="https://img.shields.io/badge/정희재-005E9D?style=flat-square&logo=&logoColor=white"/> 

유니티로 제작한 모바일 2D 타워디펜스 게임 프로젝트입니다.

> 영웅을 소환하고, 몰려오는 몬스터를 물리치세요.  
> 다양한 등급의 영웅을 배치하고, 합성할 수 있습니다.  
> 같은 진영의 영웅들을 조합해 시너지를 획득하세요.

---

## 🛠️ 주요 구현 요소
<table>
  <tr>
    <td align="center"><strong>영웅 선택 및 전투</strong></td>
    <td align="center"><strong>영웅 배치</strong></td>
    <td align="center"><strong>영웅 판매 및 합성</strong></td>
  </tr>
  <tr>
    <td><img src="./Screenshots/영웅 전투.png" width="250"/></td>
    <td><img src="./Screenshots/프로젝트 소개 2.png" width="250"/></td>
    <td><img src="./Screenshots/영웅 합성 전.png" width="250"/></td>
  </tr>
</table>

<table>
  <tr>
    <td align="center"><strong>소환 확률 강화</strong></td>
    <td align="center"><strong>행운 뽑기</strong></td>
   <td align="center"><strong>시너지</strong></td>
  </tr>
  <tr>
    <td><img src="./Screenshots/확률 강화.png" width="260"/></td>
    <td><img src="./Screenshots/행운 뽑기.png" width="250"/></td>
   <td><img src="./Screenshots/시너지 효과.png" width="250"/></td>
  </tr>

- **플레이어 조작** 구현 👉 [PlayerMove.cs](https://github.com/KALI-UM/Unity-AnimalBreakOut/blob/main/Assets/Scripts/Player/PlayerMove.cs)
   
- **구글 애드몹 연동하여 보상형 광고** 구현

- **어드레서블 에셋 시스템**을 활용하여 캐릭터 리소스 비동기 로드 구현 👉 [PlayerLoadManager.cs](https://github.com/KALI-UM/Unity-AnimalBreakOut/blob/main/Assets/Scripts/Managers/PlayerLoadManager.cs)

- **행동 트리** 구현 👉 [BehaviorTree](https://github.com/KALI-UM/Unity-AnimalBreakOut/tree/main/Assets/Scripts/BehaviourTree)
  
- **개발 툴**
  - 게임 오브젝트를 캡처해 png파일로 생성하는 **아이콘 이미지 캡처 툴** 개발 👉 [GameObjectToTexture.cs](https://github.com/KALI-UM/Unity-AnimalBreakOut/blob/main/Assets/Scripts/IconStudio/GameObjectToTexture.cs#L22)
  - **유니티 웹리퀘스트**를 활용해 최신 데이터 테이블 파일로 갱신하는 **데이터 테이블 갱신 툴** 구현 👉 [GoogleSheetManager.cs](https://github.com/KALI-UM/Unity-AnimalBreakOut/blob/main/Assets/Scripts/Managers/GoogleSheetManager.cs#L59)
  - 스테미나, 경험치, 보스 HP 등을 테스트 할 수 있는 **에디터 툴** 개발
    - 👉 [BossStatusEditor.cs](https://github.com/KALI-UM/Unity-AnimalBreakOut/blob/main/Assets/Editor/BossStatusEditor.cs)
    - 👉 [GameDataManagerEditor.cs](https://github.com/KALI-UM/Unity-AnimalBreakOut/blob/main/Assets/Editor/GameDataManagerEditor.cs)
    - 👉 [OutGameUIManagerEditor.cs](https://github.com/KALI-UM/Unity-AnimalBreakOut/blob/main/Assets/Editor/OutGameUIManagerEditor.cs)
