# todo.md

진행 상황을 기록하는 문서다. 세션이 바뀌어도 이 파일만 읽으면 이어서 작업할 수 있어야 한다.

## 운영 규칙
- 세션을 시작하면 이 파일부터 읽는다.
- "진행 중"에는 한 번에 하나의 작업만 둔다.
- 작업 상태가 바뀔 때마다 즉시 갱신한다. 세션 끝에 몰아서 쓰지 않는다.
- 세션을 마치기 전에 "세션 인계"를 갱신한다.
- 완료 조건을 모두 만족해야 "완료"로 옮긴다.
- 완료된 항목은 날짜와 함께 "완료"로 옮긴다. 20개가 넘으면 오래된 것부터 지운다.
- 사용자의 결정은 "메모"가 아니라 decision.md에 기록한다.

---

## 진행 중

<!-- 형식:
### [작업명]
- 목표: 
- 완료 조건: (확인할 수 있는 기준. 예: 로그인 성공 시 /home 으로 이동한다, test.md 체크리스트를 통과한다)
- 계획:
- 계획 승인: (승인 전 / 승인 완료, YYYY-MM-DD)
- 현재 단계: (loop.md 작업 순서 번호. 예: 4. 검증)
- 관련 파일: 
- 재시도 기록: (실패한 체크리스트 항목과 연속 실패 횟수)
- 메모: 
-->

### 뉴스레터 설정 관리
- 목표: 관리자가 뉴스레터 자동 생성 시간과 자동 발송 시간을 분 단위로 조회하고 저장할 수 있다 (D-036, D-043).
- 완료 조건:
  - 관리자가 뉴스레터 설정 관리 화면(/Admin/Settings)에서 지금 설정된 생성 시간과 발송 시간을 본다 (기본값 07:50, 08:00)
  - 생성 시간과 발송 시간을 분 단위로 바꿔 저장하면 newsletter_settings 1행이 바뀌고, 다시 열어도 바뀐 값이 나온다
  - 생성 시간이 발송 시간보다 앞서지 않으면(같거나 늦으면) 저장되지 않고 INVALID_SETTING_TIME 메시지가 나온다
  - 시간 형식이 잘못되면(HH:mm 아님, 범위 밖) INVALID_INPUT으로 응답한다
  - 일반 사용자와 비로그인 사용자는 화면에 들어갈 수 없고, API는 403 FORBIDDEN / 401 UNAUTHORIZED로 응답한다
  - test.md 완료 체크리스트(1~15)를 통과한다
- 계획:
  1. DB: newsletter_settings 엔티티 매핑 (테이블과 기본 1행은 이미 있음)
  2. 화면: Settings_screen.md와 시안 docs/mockups/settings.html 작성 → 사용자 승인 → 화면만 구현 → 화면 확인 → 사용자 확인. 시간은 시·분 선택 상자로 입력한다 (D-079)
  3. 기능: GET /api/newsletter-settings, POST /api/newsletter-settings (관리자만), settings.js(fetch)
  4. 테스트: 설정 서비스(조회, 저장, 생성=발송, 생성>발송, 1분 차이, 00:00·23:59 경계, 형식 오류)의 동등 분할·경계값 테스트
- 계획 승인: 승인 완료, 2026-10-07
- 현재 단계: 4. 검증 (사용자 최종 확인 대기)
- 관련 파일: src/Newsletter/(Controllers/AdminController.cs, Controllers/Api/NewsletterSettingsController.cs, Models/Entities/NewsletterSetting.cs, Models/Dtos/, Data/NewsletterDbContext.cs, Services/, Views/Admin/Settings.cshtml, Settings_screen.md, wwwroot/js/settings.js, wwwroot/css/settings.css), docs/mockups/settings.html, tests/Newsletter.Tests/, docs/structure.md, docs/api.md
- 재시도 기록: 없음
- 메모:
  - 2026-10-11: 화면 확인 통과. 기능 구현(API 2개, NewsletterSettingService, settings.js). 빌드 경고·오류 0, dotnet test 141개 통과(차단 풀려 dotnet test 다시 사용). 브라우저 확인(바로 검사, 저장·새로고침 유지, INVALID_SETTING_TIME·INVALID_INPUT, 401, 일반 사용자 403·화면 이동) 후 설정을 07:50/08:00으로 되돌리고 임시 계정 삭제

---

## 백로그

우선순위가 높은 순서로 적는다. overview.md 핵심 기능의 우선순위를 기준으로 적는다.

"진행 중"에서 옮긴 작업은 "진행 중" 형식의 내용을 그대로 유지한 채 옮긴다.

- [ ] 공휴일 등록 (특일정보 API)
- [ ] 뉴스레터 직접 생성/발송 (임시 데이터)
- [ ] 뉴스레터 자동 생성/발송 (스케줄러)
- [ ] 뉴스레터 히스토리
- [ ] 석유 데이터 출처 결정 후 실제 API 연결 (출처 결정 필요, D-050)

---

## 막힌 일

사용자의 결정, 외부 조건, 또는 반복 실패(loop.md "종료 조건") 때문에 진행할 수 없는 항목이다.

- (없음)

---

## 세션 인계

- **마지막 갱신**: 2026-10-11
- **현재 상태**: 회원가입, 로그인, 수신자 관리 작업을 완료했다(아래 "완료"). 뉴스레터 설정 관리를 진행 중이다: 계획 승인, D-079, 설계·시안 승인, 화면 코드(Settings.cshtml, settings.css, AdminController.Settings) 작성·빌드 성공. 화면 확인은 애플리케이션 제어 정책의 dll 차단으로 막혔다("막힌 일"). 2026-10-07 세션은 사용자 요청으로 여기서 종료했다.
  - 코드: src/Newsletter(MVC, .NET 10), tests/Newsletter.Tests(xUnit v3, 118개 통과). DB는 로컬 SQL Server 2025 Developer의 newsletter.
  - 계정: wjdgkr3386(Admin, D-073)만 있다. 확인용 계정이 필요하면 무작위 비밀번호로 임시 계정을 만들어 확인 후 지운다(비밀번호는 scratchpad에만 두고 채팅에 쓰지 않는다).
  - 실행: 앱 내 브라우저는 .claude/launch.json의 "newsletter"(http://localhost:5044), 시안은 "mockups"(포트 5500).
  - main에 병합·푸시 완료(91c75f7, 2026-10-11). 빌드 결과물(bin, obj)은 저장소에서 뺐다. 커밋·푸시는 사용자가 직접 한다(git.md).
- **다음에 할 일**: "막힌 일"의 dll 차단을 사용자와 해결한다(스마트 앱 컨트롤 끄기, Visual Studio 실행 확인, 다시 시도 중 선택). 해결되면 설정 화면 확인(loop.md 3-4, 3-5) → 기능(GET/POST /api/newsletter-settings, settings.js) → 테스트를 진행한다. 설정 화면 작업분은 아직 커밋하지 않았다.
- **주의 사항**:
  - 석유 데이터 출처가 미정이다(D-050). 정해질 때까지 임시 데이터로 개발한다.
  - User Secrets 등 AppData 아래 설정은 사용자가 cmd에서 직접 넣는다(security.md). Claude 데스크톱 앱(MSIX)에서는 AI가 쓴 AppData 파일이 앱 전용 가상 폴더(AppData\Local\Packages\Claude_pzs8sxrjxfjjc\LocalCache\Roaming)에 저장되어 서로 다른 파일을 보게 된다. 키 3개(ConnectionStrings:Newsletter, Mail:SenderAddress, Mail:AppPassword)는 사용자가 넣었다.
  - 사용자가 직접 준비할 것: 공공데이터포털 특일정보 API 키(공휴일 등록 작업 전).
  - .NET 10 SDK에서 dotnet test는 루트 global.json(Microsoft.Testing.Platform)이 있어야 동작한다. 앱 실행 중에는 빌드가 파일 잠금으로 실패하므로 미리보기 서버를 멈추고 빌드한다.
  - 2026-10-07에 Windows 애플리케이션 제어 정책이 새로 빌드한 exe·dll을 막았다가 2026-10-11에는 풀렸다. 다시 막히면 앱은 launch.json의 -p:UseAppHost=false로 실행하고, 보안 설정은 바꾸지 않는다.

---

## 완료

- 2026-10-07 수신자 관리: 수신자 목록(이름·이메일, 10명씩 페이징), 추가·삭제 팝업(여러 명 선택, 검색, 선택 유지, 전체 선택), API 4개(하나라도 오류면 전체 취소), 로그인 후 본문 960px 가운데(D-078). D-075~D-078. 빌드 경고·오류 0, 테스트 118개 통과, 사용자 확인 완료
- 2026-10-07 로그인 (일반 사용자/관리자 구분, 좌측 네비게이션): 쿠키 인증(60분, 요청 시 연장, HttpOnly, SameSite=Strict), 관리자 전용 권한, 로그인·로그아웃 API, 로그인 화면, 좌측 네비게이션 레이아웃(사용자 정보 표시 안 함 D-074), 빈 화면 2개(D-070), 루트 주소·로그인 사용자 이동(D-071, D-072), 관리자 계정 지정(D-073). 빌드 경고·오류 0, 테스트 103개 통과, 사용자 화면 확인 완료
- 2026-10-07 회원가입 (이메일 인증 코드 포함): 프로젝트 생성, DB 생성, API 3개(인증 코드 발송·확인, 회원가입), Gmail 발송, 화면(실시간 검사 D-067, 5번 틀리면 무효 D-068, 발송 즉시 화면 전환 D-069). 빌드 경고·오류 0, 테스트 88개 통과, 사용자가 실제 메일 수신과 가입 확인
