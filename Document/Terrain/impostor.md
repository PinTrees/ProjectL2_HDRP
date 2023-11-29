## 식생 오브젝트 임포스터 생성
- 해당 문서는 식생오브젝트의 임포스터 생성방법에 대한 문서입니다. 일반 오브젝트의 임포스터 생성은 impostor_hdrplit.md로 이동

- 임포스터에 대한 기반 지식
- https://www.youtube.com/watch?v=G0ILW1cwuTc
  
- 해당 프로젝트는 Amplify Impostors + TVE 기반으로 구축되었습니다.

### 1. 식생 오브젝트 초기 세팅
- 식생 오브젝트가 TVE > Plant Surface Shader를 사용하는지 확인합니다.
- 해당 쉐이더가 아닐경우 변경작업이 필요합니다. Terrain/biome_tve.md 파일로 이동
<img width="1200" alt="image" src="https://github.com/PinTrees/ProjectL2_HDRP/assets/59812031/8897d360-8547-41f9-8f48-71f3816179ca">

### 2. Amplify Impostor 초기세팅
- 최상위 부모에 Amplify Impostor 스크립트를 추가합니다.
- + 버튼을 눌러 해당 오브젝트가 있는 위치에 해당 오브젝트의 이름으로 파일을 생성합니다.
- 가능한 오브젝트의 원래 이름을 사용하고 변경하지 않도록 합니다.
<img width="1200" alt="image" src="https://github.com/PinTrees/ProjectL2_HDRP/assets/59812031/5944115d-b44a-4236-88df-ea6fff3eb825">

### 3. 임포스터 생성
- BakeType을 Octahedron으로 변경합니다.
- Texture Size를 오브젝트의 크기에 맞게 설정합니다. 단 8k는 사용하지 않습니다.
- Bake Preset을 TVE Plants (Octahedron) 으로 변경합니다.
- Bake Impostor 버튼을 눌러 임포스터를 베이킹 합니다.
<img width="1200" alt="image" src="https://github.com/PinTrees/ProjectL2_HDRP/assets/59812031/e82654d8-5877-4046-9014-f8f8a5a18a33">


### 4. Shader 세팅
- 오브젝트의 하위에 생성된 Impostor 파일의 쉐이더와 본 식생 쉐이더 간 설정 공유를 위해 드래그앤 드랍으로 데이터를 넘겨줍니다.
- 오브젝트를 둘러보며 임포스터와 본 오브젝트간 괴리감이 없도록 쉐이더 값을 미세 조정합니다.
<img width="1200" alt="image" src="https://github.com/PinTrees/ProjectL2_HDRP/assets/59812031/6d9a8d73-fcdf-46ce-b561-bc083d4818f0">


### 5. LOD 세팅
- LOD 수준을 변경하며 가장 적절한 위치에 임포스터가 나타나도록 미세조정후 프리팹을 저장합니다.
<img width="1207" alt="image" src="https://github.com/PinTrees/ProjectL2_HDRP/assets/59812031/58defd3e-138f-4238-867e-c957b19e0d94">


### 결과
<img width="1247" alt="image" src="https://github.com/PinTrees/ProjectL2_HDRP/assets/59812031/74e30bc3-4846-4832-802d-87e9dd4d6fff">
