# structure.md

해당 파일은 폴더 구조와 간단한 설명을 정리한 파일이다.

---

## 규칙

1. 이 파일에서 수정할 수 있는 곳은 "변경 가능" 목록뿐이다. 이 규칙과 "변경 불가능" 목록을 포함한 나머지 부분은 수정하지 않는다.
2. "변경 가능" 목록
   - 목록의 항목(경로, 파일명, 설명)은 추가, 수정, 삭제할 수 있다.
   - 폴더나 파일을 만들거나 옮기거나 지우면, 실제 구조와 맞도록 이 목록을 바로 갱신한다. `프로젝트/backend/`와 `프로젝트/frontend/` 하위 구조가 바뀔 때도 같다.
3. "변경 불가능" 목록
   - 목록에 적힌 경로와 파일명은 수정하지 않는다.
   - 목록에 항목을 추가하거나 삭제하지 않는다.
   - 목록에 적힌 실제 파일을 옮기거나 이름을 바꾸거나 삭제하지 않는다. 파일 내용은 각 문서의 규칙에 따라 수정할 수 있다.

---

## 1. 폴더 구조

### 변경 가능
|폴더구조|설명|
|---|---|
|프로젝트/docs/|설계·진행 문서 폴더|
|프로젝트/README.md|저장소 소개|

<!-- 아래는 아직 만들지 않은 계획 구조다(D-056, D-057). 실제로 만들 때 "(예정)"을 지우고, 파일 단위 항목을 추가한다 -->
|폴더구조|설명|
|---|---|
|프로젝트/Newsletter.sln|솔루션 파일 (예정)|
|프로젝트/src/Newsletter/|ASP.NET Core MVC 웹 프로젝트 (예정)|
|프로젝트/src/Newsletter/Program.cs|앱 시작, 서비스 등록, 쿠키 인증·스케줄러 설정 (예정)|
|프로젝트/src/Newsletter/appsettings.json|비밀 정보를 뺀 설정. 비밀 정보는 User Secrets에 둔다 (예정)|
|프로젝트/src/Newsletter/Controllers/|화면 틀(Razor 뷰)을 반환하는 화면 컨트롤러 (예정)|
|프로젝트/src/Newsletter/Controllers/Api/|JSON API 컨트롤러 (/api/...) (예정)|
|프로젝트/src/Newsletter/Views/Account/|로그인, 회원가입 화면 (예정)|
|프로젝트/src/Newsletter/Views/History/|히스토리 목록, 본문 화면 (예정)|
|프로젝트/src/Newsletter/Views/Admin/|관리자 화면 (발송, 수신자, 설정) (예정)|
|프로젝트/src/Newsletter/Views/Shared/|공통 레이아웃, 좌측 네비게이션 (예정)|
|프로젝트/src/Newsletter/Models/Entities/|DB 테이블과 매핑되는 엔티티 클래스 (예정)|
|프로젝트/src/Newsletter/Models/Dtos/|API 요청·응답 클래스 (예정)|
|프로젝트/src/Newsletter/Data/|EF Core DbContext (예정)|
|프로젝트/src/Newsletter/Services/|비즈니스 로직 (회원, 메일, 뉴스레터, 공휴일 등) (예정)|
|프로젝트/src/Newsletter/BackgroundJobs/|BackgroundService 스케줄러 (자동 생성/발송, 공휴일 등록) (예정)|
|프로젝트/src/Newsletter/wwwroot/css/|화면별 CSS. 파일명은 연결된 화면 파일명과 같게 한다 (예정)|
|프로젝트/src/Newsletter/wwwroot/js/|화면별 JS (API 호출). 파일명은 연결된 화면 파일명과 같게 한다 (예정)|
|프로젝트/src/Newsletter/wwwroot/lib/|부트스트랩 등 MVC 템플릿 기본 라이브러리 (예정)|
|프로젝트/tests/Newsletter.Tests/|xUnit v3 테스트 프로젝트 (예정)|

### 변경 불가능
- 프로젝트/docs/coding_rule.md
- 프로젝트/docs/database.md
- 프로젝트/docs/screen.md
- 프로젝트/docs/guide.md
- 프로젝트/docs/loop.md
- 프로젝트/docs/spec.md
- 프로젝트/docs/structure.md
- 프로젝트/docs/test.md
- 프로젝트/docs/todo.md
- 프로젝트/docs/overview.md
- 프로젝트/docs/api.md
- 프로젝트/docs/decision.md
- 프로젝트/docs/security.md
- 프로젝트/CLAUDE.md
