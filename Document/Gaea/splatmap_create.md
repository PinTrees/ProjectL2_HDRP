## Unity Terrain 기반 스플랫 맵 생성 (Gaea)
- 파일만 존재할 경우 Splat 맵 생성에 대한 문서입니다. Gaea를 통해 자체 지형을 생성하려는 경우 landscape_create.md 파일로 이동

### 1. HeightMap 파일 로드
<img width="1200" alt="image" src="https://github.com/PinTrees/ProjectL2_HDRP/assets/59812031/d18ef59d-34bc-4098-9f1d-0d66b1bab131">

### 2. Curve 높이 수준 재설정
- 이상적인 형태를 획득할 때까지 반복
- 외부에서 제작된 HeightMap의 경우 높이 수준이 불안정 할 수 있으므로 변경이 필요합니다.
- Curve의 시작지점 (가장 낮은 영역), Curve의 끝 지점 (가장 높은 영역)
<img width="1205" alt="image" src="https://github.com/PinTrees/ProjectL2_HDRP/assets/59812031/38975c72-f615-4a25-b209-ac2e996723e5">

### 3. 높이수준이 변경되었으므로 HeightMap을 추가하고 출력설정에 추가
- 체크 표시로 변경합니다.
<img width="1200" alt="image" src="https://github.com/PinTrees/ProjectL2_HDRP/assets/59812031/aa2be1d8-d009-4ef0-a614-95f24226977d">

### 4. SplatMap 생성 기초 텍스쳐 및 (Slope)경사, (Protrucsion)돌출 노드 추가
- 경사도를 변경하여 지형의 가파른 경사영역을 모두 추가합니다.
- 경사노드와 돌출 노드를 합쳐 경사의 시작영역을 부드럽게 만듭니다.
<img width="1200" alt="image" src="https://github.com/PinTrees/ProjectL2_HDRP/assets/59812031/a12ba4f3-b65d-47ef-a449-b41a35cd8194">

### 5. Soil, Flow 노드 추가
- Flow 노드로 강 주위 영역 설정합니다.
- 두 노드를 합쳐 자연스러운 토양 영역을 생성합니다.
<img width="1200" alt="image" src="https://github.com/PinTrees/ProjectL2_HDRP/assets/59812031/f4616351-c8b0-4a6b-abde-51a9e962b57d">

### 5. 최종 SplatMap을 추가합니다.
- RGB 순서를 지켜서 노드에 추가
- R: 텍스쳐
- G: 경사 + 돌출
- B: 토양 + 강
- A: 유니티 상에서의 처리 오류문제로 사용하지 않습니다.
- 후처리 효과를 Equalize 로 변경합니다.
- RGB Clamp 를 사용으로 변경합니다.
- Autolevel 설정을 사용으로 변경합니다.
<img width="1200" alt="image" src="https://github.com/PinTrees/ProjectL2_HDRP/assets/59812031/9766202d-a3d6-484a-ab5a-734123170f36">

### 7. 파일 출력
- 1. 파일 이름을 변경합니다. (높이맵: Height, 스플랫맵: Splat) 같을 경우 그대로 유지
- 확장자를 .exr로 변경합니다.
- 2. 출력 크기를 2048 이상으로 사용합니다.
<img width="1207" alt="image" src="https://github.com/PinTrees/ProjectL2_HDRP/assets/59812031/bae5eed0-3a77-45a4-b522-f8c80ed0326d">


