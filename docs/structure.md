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
|프로젝트/docs/reference.md|reference 폴더 참고자료 목록 (파일명, 설명, 관련 작업). AI가 폴더를 볼 때 갱신|
|프로젝트/docs/mockups/|화면 시안 HTML. 서버 없이 바로 열어 보는 파일 (D-066)|
|프로젝트/docs/mockups/register.html|회원가입 화면 시안|
|프로젝트/docs/mockups/login.html|로그인 화면 시안|
|프로젝트/docs/mockups/app_layout.html|로그인 후 공통 레이아웃(좌측 네비게이션) 시안. 빈 화면(히스토리 목록, 뉴스레터 발송) 시안을 겸함|
|프로젝트/docs/mockups/recipients.html|수신자 관리 화면 시안 (추가·삭제 팝업 포함)|
|프로젝트/docs/mockups/settings.html|뉴스레터 설정 관리 화면 시안|
|프로젝트/.claude/launch.json|앱 내 브라우저 미리보기 서버 설정 (mockups: 시안 보기 포트 5500, newsletter: 웹 앱 실행 포트 5044. 애플리케이션 제어 정책이 exe를 막아 UseAppHost=false로 dll 실행. 2026-10-11부터 차단은 풀렸지만 설정은 그대로 둠)|
|프로젝트/.claude/mockup_server.js|시안 미리보기용 정적 파일 서버 (Node 기본 모듈만 사용, 127.0.0.1 전용)|
|프로젝트/README.md|저장소 소개|
|프로젝트/.gitignore|git 제외 목록 (dotnet new gitignore)|
|프로젝트/Newsletter.sln|솔루션 파일|
|프로젝트/global.json|dotnet test가 Microsoft.Testing.Platform 실행기를 쓰도록 지정 (.NET 10 SDK, xUnit v3)|
|프로젝트/src/Newsletter/|ASP.NET Core MVC 웹 프로젝트|
|프로젝트/src/Newsletter/Newsletter.csproj|웹 프로젝트 파일 (패키지 참조, UserSecretsId)|
|프로젝트/src/Newsletter/Program.cs|앱 시작, 서비스 등록, 쿠키 인증·스케줄러 설정|
|프로젝트/src/Newsletter/appsettings.json|비밀 정보를 뺀 설정 (Mail: SmtpHost, SmtpPort, SenderName / Holiday: BaseUrl / HttpClient 로그는 Warning부터). 비밀 정보는 User Secrets에 둔다|
|프로젝트/src/Newsletter/secrets.example.json|User Secrets에 넣을 키 이름 목록 (값 없음). 사용자가 cmd에서 직접 넣는다|
|프로젝트/src/Newsletter/appsettings.Development.json|개발 환경 설정|
|프로젝트/src/Newsletter/Properties/launchSettings.json|로컬 실행 설정 (http://localhost:5044)|
|프로젝트/src/Newsletter/Controllers/|화면 틀(Razor 뷰)을 반환하는 화면 컨트롤러|
|프로젝트/src/Newsletter/Controllers/HomeController.cs|루트 주소(/) 이동 (로그인 여부·권한별, D-071), 오류 화면|
|프로젝트/src/Newsletter/Controllers/AccountController.cs|로그인, 회원가입 화면 컨트롤러. GET /Account/Login, /Account/Register (로그인했으면 첫 화면으로, D-072)|
|프로젝트/src/Newsletter/Controllers/HistoryController.cs|히스토리 화면 컨트롤러 (로그인 필요). GET /History|
|프로젝트/src/Newsletter/Controllers/AdminController.cs|관리자 화면 컨트롤러 (관리자만). GET /Admin/Send, /Admin/Recipients, /Admin/Settings|
|프로젝트/src/Newsletter/Controllers/Api/|JSON API 컨트롤러 (/api/...)|
|프로젝트/src/Newsletter/Controllers/Api/ApiControllerBase.cs|API 공통 (api.md 공통 응답 형식 변환, 안티포저리 검사 적용)|
|프로젝트/src/Newsletter/Controllers/Api/EmailVerificationsController.cs|POST /api/email-verifications, /api/email-verifications/confirm|
|프로젝트/src/Newsletter/Controllers/Api/UsersController.cs|POST /api/users (회원가입), GET /api/users (회원 목록, 관리자만)|
|프로젝트/src/Newsletter/Controllers/Api/AuthController.cs|POST /api/auth/login, /api/auth/logout (쿠키 로그인·로그아웃)|
|프로젝트/src/Newsletter/Controllers/Api/RecipientsController.cs|GET /api/recipients, POST /api/recipients, POST /api/recipients/delete (관리자만)|
|프로젝트/src/Newsletter/Controllers/Api/NewsletterSettingsController.cs|GET, POST /api/newsletter-settings (관리자만)|
|프로젝트/src/Newsletter/Filters/ApiAntiforgeryAttribute.cs|API POST 요청의 RequestVerificationToken 헤더 검사. 실패 시 INVALID_INPUT JSON 응답|
|프로젝트/src/Newsletter/Views/_ViewImports.cshtml|뷰 공통 using, 태그 헬퍼|
|프로젝트/src/Newsletter/Views/_ViewStart.cshtml|뷰 공통 레이아웃 지정|
|프로젝트/src/Newsletter/Views/Account/|로그인, 회원가입 화면|
|프로젝트/src/Newsletter/Views/Account/Register.cshtml|회원가입 화면 (Razor 뷰, 화면 틀)|
|프로젝트/src/Newsletter/Views/Account/Register_screen.md|회원가입 화면 설계 문서|
|프로젝트/src/Newsletter/Views/Account/Login_screen.md|로그인 화면 설계 문서|
|프로젝트/src/Newsletter/Views/Account/Login.cshtml|로그인 화면 (Razor 뷰, 화면 틀)|
|프로젝트/src/Newsletter/Views/History/|히스토리 목록, 본문 화면|
|프로젝트/src/Newsletter/Views/History/Index_screen.md|히스토리 목록 화면 설계 문서 (지금은 빈 화면, D-070)|
|프로젝트/src/Newsletter/Views/History/Index.cshtml|히스토리 목록 화면 (지금은 빈 화면)|
|프로젝트/src/Newsletter/Views/Admin/|관리자 화면 (발송, 수신자, 설정)|
|프로젝트/src/Newsletter/Views/Admin/Send_screen.md|뉴스레터 발송 화면 설계 문서 (지금은 빈 화면, D-070)|
|프로젝트/src/Newsletter/Views/Admin/Send.cshtml|뉴스레터 발송 화면 (지금은 빈 화면)|
|프로젝트/src/Newsletter/Views/Admin/Recipients_screen.md|수신자 관리 화면 설계 문서|
|프로젝트/src/Newsletter/Views/Admin/Recipients.cshtml|수신자 관리 화면 (Razor 뷰, 추가·삭제 팝업 포함)|
|프로젝트/src/Newsletter/Views/Admin/Settings_screen.md|뉴스레터 설정 관리 화면 설계 문서|
|프로젝트/src/Newsletter/Views/Admin/Settings.cshtml|뉴스레터 설정 관리 화면 (시·분 선택 상자)|
|프로젝트/src/Newsletter/Views/Shared/|공통 레이아웃(_Layout: 비회원 화면용, _AppLayout: 로그인 후 화면용), 오류 화면(Error), 검증 스크립트 부분 뷰|
|프로젝트/src/Newsletter/Views/Shared/_AppLayout_screen.md|로그인 후 공통 레이아웃(좌측 네비게이션) 설계 문서|
|프로젝트/src/Newsletter/Views/Shared/_AppLayout.cshtml|로그인 후 공통 레이아웃 (좌측 네비게이션, 권한별 메뉴, 로그아웃)|
|프로젝트/src/Newsletter/Models/ErrorViewModel.cs|MVC 템플릿 기본 오류 화면 모델|
|프로젝트/src/Newsletter/Models/Entities/|DB 테이블과 매핑되는 엔티티 클래스 (User, UserRole, EmailVerification, Recipient, NewsletterSetting, Holiday)|
|프로젝트/src/Newsletter/Models/Dtos/|API 요청·응답 클래스 (공통 응답 ApiSuccessResponse, ApiErrorResponse, ApiErrorBody, EmptyData와 회원가입·로그인·수신자·뉴스레터 설정 API 요청·응답)|
|프로젝트/src/Newsletter/Data/NewsletterDbContext.cs|EF Core DbContext (users, email_verifications, recipients, newsletter_settings, holidays 매핑. 마이그레이션 없음)|
|프로젝트/src/Newsletter/Services/|비즈니스 로직. ErrorCodes, ServiceError, ServiceResult, InputValidator(입력값 검증), VerificationRules(인증 코드 만료), MailOptions, IMailSender, GmailMailSender, IEmailVerificationService, EmailVerificationService, IUserService, UserService, HomePathResolver(권한별 첫 화면), AuthPolicies(권한 정책 이름), IAuthService, AuthService(로그인 확인), IRecipientService, RecipientService(수신자 관리), INewsletterSettingService, NewsletterSettingService(생성·발송 시간 설정), KoreanTime(KST 날짜), HolidayOptions, HolidayInfo, IHolidayApiClient, SpecialDayApiClient(특일정보 API), IHolidayService, HolidayService(공휴일 동기화·발송 제외일 판단), HolidaySyncResult|
|프로젝트/src/Newsletter/BackgroundJobs/|BackgroundService 스케줄러|
|프로젝트/src/Newsletter/BackgroundJobs/HolidayRegistrationJob.cs|1시간마다 확인해 오늘(KST) 아직 동기화하지 않았으면 올해 공휴일을 받아 없는 날짜만 추가 (D-080, D-082)|
|프로젝트/src/Newsletter/wwwroot/css/site.css|공통 CSS (배경색)|
|프로젝트/src/Newsletter/wwwroot/css/register.css|회원가입 화면 CSS|
|프로젝트/src/Newsletter/wwwroot/css/login.css|로그인 화면 CSS|
|프로젝트/src/Newsletter/wwwroot/css/app_layout.css|로그인 후 공통 레이아웃 CSS (좌측 네비게이션, 본문 960px 가운데)|
|프로젝트/src/Newsletter/wwwroot/css/recipients.css|수신자 관리 화면 CSS|
|프로젝트/src/Newsletter/wwwroot/css/settings.css|뉴스레터 설정 관리 화면 CSS|
|프로젝트/src/Newsletter/wwwroot/css/|화면별 CSS. 파일명은 연결된 화면 파일명과 같게 한다|
|프로젝트/src/Newsletter/wwwroot/js/site.js|MVC 템플릿 기본 공통 JS|
|프로젝트/src/Newsletter/wwwroot/js/|화면별 JS (API 호출). 파일명은 연결된 화면 파일명과 같게 한다|
|프로젝트/src/Newsletter/wwwroot/js/register.js|회원가입 화면 JS (입력값 검사, 인증 코드 발송·확인, 카운트다운, 가입 API 호출)|
|프로젝트/src/Newsletter/wwwroot/js/login.js|로그인 화면 JS (빈 칸 검사, 로그인 API 호출, 권한별 이동)|
|프로젝트/src/Newsletter/wwwroot/js/app_layout.js|로그인 후 공통 레이아웃 JS (로그아웃)|
|프로젝트/src/Newsletter/wwwroot/js/recipients.js|수신자 관리 화면 JS (목록·페이징, 추가·삭제 팝업, 검색, 선택 유지)|
|프로젝트/src/Newsletter/wwwroot/js/settings.js|뉴스레터 설정 관리 화면 JS (불러오기, 바로 검사, 저장)|
|프로젝트/src/Newsletter/wwwroot/lib/|부트스트랩 5.3.3, jQuery 등 MVC 템플릿 기본 라이브러리|
|프로젝트/src/Newsletter/wwwroot/favicon.ico|파비콘|
|프로젝트/tests/Newsletter.Tests/|xUnit v3 테스트 프로젝트|
|프로젝트/tests/Newsletter.Tests/Newsletter.Tests.csproj|테스트 프로젝트 파일 (웹 프로젝트 참조)|
|프로젝트/tests/Newsletter.Tests/TestHelpers/|테스트 도구 (FakeTimeProvider, FakeMailSender, FakeHolidayApiClient, TestDbContextFactory: EF Core InMemory DB)|
|프로젝트/tests/Newsletter.Tests/Services/|서비스 테스트 (InputValidatorTests, VerificationRulesTests, EmailVerificationServiceTests, UserServiceTests, AuthServiceTests, HomePathResolverTests, RecipientServiceTests, NewsletterSettingServiceTests, SpecialDayApiClientTests, HolidayServiceTests, HolidayRegistrationJobTests, KoreanTimeTests)|

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
- 프로젝트/docs/git.md
- 프로젝트/docs/reference/
- 프로젝트/CLAUDE.md
