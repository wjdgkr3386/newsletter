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

(없음)

---

## 백로그

우선순위가 높은 순서로 적는다. overview.md 핵심 기능의 우선순위를 기준으로 적는다.

"진행 중"에서 옮긴 작업은 "진행 중" 형식의 내용을 그대로 유지한 채 옮긴다.

- [ ] 회원가입 (이메일 인증 코드 포함)
- [ ] 로그인 (일반 사용자/관리자 구분, 좌측 네비게이션)
- [ ] 수신자 관리
- [ ] 뉴스레터 설정 관리
- [ ] 공휴일 등록 (특일정보 API)
- [ ] 뉴스레터 직접 생성/발송 (임시 데이터)
- [ ] 뉴스레터 자동 생성/발송 (스케줄러)
- [ ] 보낸 메일 확인
- [ ] 뉴스레터 히스토리
- [ ] 석유 데이터 출처 결정 후 실제 API 연결 (출처 결정 필요, D-050)

---

## 막힌 일

사용자의 결정, 외부 조건, 또는 반복 실패(loop.md "종료 조건") 때문에 진행할 수 없는 항목이다.

- (없음)

---

## 세션 인계

- **마지막 갱신**: 2026-10-03
- **현재 상태**: 설계 단계. 요구사항을 decision.md(D-001~D-050)와 overview.md(전 항목)에 확정했다. spec.md는 스택 일부만 채웠다. 백로그는 overview.md 우선순위대로 만들었다. 코드는 아직 없다.
  - 사용자가 나머지 설계 문서를 먼저 정리하기로 했다(백로그 작업 전).
  - 그래서 아래 초안을 사용자에게 제시했고, 답변을 기다리는 중이다. 승인 전이므로 아직 어느 문서에도 반영하지 않았다.
  - **A. spec.md 기술 선택 초안**
    - 기술: EF Core 10, 쿠키 인증(Identity 전체 미사용), BCrypt.Net-Next, MailKit(Gmail SMTP), xUnit, .NET User Secrets
    - 질문 1: SQL Server 설치 종류를 정한다. (가) Express, (나) Developer, (다) LocalDB 중 하나. 이 PC에는 SQL Server 서비스와 LocalDB가 없다.
  - **B. database.md 테이블 초안**
    - 테이블: users(login_id/email unique, role User/Admin), email_verifications(code char(6), expires_at, is_verified), recipients(user_id unique), newsletters(status Generated/GenerationFailed/Sent/SendFailed, 생성 실패도 행으로 남김), send_logs(수신자별 결과, email 함께 저장), newsletter_settings(1행, KST time, last_generate_date/last_send_date로 중복 실행 방지), holidays(holiday_date unique)
    - 질문 2: 입력값 제한. 아이디는 영문 소문자+숫자 4~20자, 비밀번호는 8~64자에 영문·숫자 각 1자 이상, 이름은 1~50자, 이메일은 최대 254자.
    - 질문 3: 뉴스레터 제목 형식을 "[석유 뉴스레터] YYYY-MM-DD"로 할지.
    - 질문 4: send_logs가 users를 FK로 참조하면서 이메일도 저장하는 방식으로 할지.
    - 질문 5: 수신자를 삭제해도 과거 발송 기록을 남길지(제안: 남긴다).
  - **C. structure.md 초안**
    - 구조: Newsletter.sln, src/Newsletter.Web(Controllers, Views, Models, Data, Services, BackgroundJobs, wwwroot), tests/Newsletter.Tests
    - 질문 6: backend/frontend로 나누지 않고 한 MVC 프로젝트로 구성해도 되는지.
  - **D. api.md 초안**
    - 화면은 MVC 폼 전송으로 만든다. JSON API는 POST /api/email-verifications(코드 발송)와 POST /api/email-verifications/confirm(코드 확인) 두 개만 둔다.
    - 질문 7: 나머지 기능을 폼 전송으로 할지, 모두 JSON API로 통일할지.
- **다음에 할 일**: 사용자에게 질문 1~7의 답을 받는다. 답을 decision.md에 기록한 뒤 spec.md, database.md(실제 CREATE TABLE 코드 포함), structure.md, api.md를 작성한다. 그다음 백로그 맨 위(회원가입)로 loop.md 1번(계획)을 진행한다.
- **주의 사항**:
  - 석유 데이터 출처가 미정이다(D-050). 정해질 때까지 임시 데이터로 개발한다.
  - 사용자가 직접 준비할 것: Gmail 앱 비밀번호(2단계 인증 필요), 공공데이터포털 특일정보 API 키, SQL Server 설치.
  - structure.md "변경 가능" 목록에는 아직 예시 항목만 있다.

---

## 완료

- {{YYYY-MM-DD}} {{작업명}}
