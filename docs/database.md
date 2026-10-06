# database.md

데이터 모델의 기준 문서다. 스키마를 바꾸는 작업에서는 이 문서도 함께 갱신한다.

이 문서와 실제 스키마가 다르면 사용자에게 알린다.

---

## 1. 개요

- **DB**: Microsoft SQL Server (Developer 에디션), 데이터베이스 이름 `newsletter`
- **스키마 생성**: 아래 "4. 실제 사용할 테이블 생성 코드"의 SQL을 직접 실행해 만든다. EF Core는 이미 만든 테이블에 매핑만 하고 마이그레이션은 쓰지 않는다.
- **시각 저장**: 시각(`datetime2`)은 UTC로 저장한다. 뉴스레터 설정 시간(`time`)과 날짜(`date`)는 KST 기준 값이다.

---

## 2. 네이밍 규칙

| 대상 | 규칙 | 예 |
|---|---|---|
| 테이블 | snake_case, 복수형 | `users`, `order_items` |
| 컬럼 | snake_case | `created_at` |
| 기본 키 | `id` | |
| 외래 키 | `{단수 테이블명}_id` | `user_id` |
| 인덱스 | `idx_{테이블}_{컬럼}` | `idx_users_email` |
| 불리언 | `is_` 접두사 | `is_active` |

### 공통 컬럼
모든 테이블에 기본으로 둔다
- `id`: bigint identity(1,1)
- `created_at`: 생성 시각 (UTC)
- `updated_at`: 수정 시각 (UTC)
- `deleted_at`: 소프트 삭제가 필요한 테이블만

---

## 3. 테이블 정의

<!-- 테이블마다 아래 형식을 복사해서 쓴다 -->
```sql
create table 테이블명(
`컬럼` 타입 제약 -- 설명
);
```

<!-- 예시 -->
```sql
create table users(
 `id` bigint auto_increment primary key, -- 기본키
 `name` varchar(20) not null -- 이름
);
```

---

## 4. 실제 사용할 테이블 생성 코드
<!-- 실제 테이블 생성 코드를 이 아래에 작성 -->

- 실행 순서: 아래에 적힌 순서대로 실행한다. (외래 키가 참조하는 테이블을 먼저 만든다)
- 소프트 삭제(`deleted_at`)가 필요한 테이블은 없다. 회원 탈퇴가 없고(D-021), 수신자는 삭제해도 발송 기록이 users를 참조하므로(D-054, D-055) 수신자 행은 바로 지운다.
- 입력값 형식(아이디 문자 구성, 비밀번호 규칙, 이메일 형식)은 서버 코드에서 검증한다. DB는 길이와 값 목록만 제한한다.

### 4.1 users (회원)
```sql
create table users(
 id bigint identity(1,1) not null primary key, -- 기본키
 login_id varchar(20) not null, -- 아이디. 영문 소문자+숫자 6~20자 (D-052)
 password_hash char(60) not null, -- BCrypt 해시 (60자 고정). 비밀번호 원문은 8~20자 (D-052)
 email varchar(50) not null, -- 이메일. 기본 이메일 형식, 최대 50자 (D-052)
 name nvarchar(30) not null, -- 이름. 2~30자 (D-052)
 role varchar(10) not null constraint df_users_role default 'User', -- 권한. User / Admin
 created_at datetime2(0) not null constraint df_users_created_at default sysutcdatetime(), -- 생성 시각 (UTC)
 updated_at datetime2(0) not null constraint df_users_updated_at default sysutcdatetime(), -- 수정 시각 (UTC)
 constraint ck_users_role check (role in ('User', 'Admin'))
);
create unique index idx_users_login_id on users(login_id); -- 아이디 중복 불가 (D-032)
create unique index idx_users_email on users(email); -- 이메일 중복 불가 (D-032)
```
- 관리자 계정은 개발자가 INSERT 문으로 만든다(D-009). `role`에 `'Admin'`, `password_hash`에 BCrypt 해시를 넣는다.

### 4.2 email_verifications (이메일 인증 코드)
```sql
create table email_verifications(
 id bigint identity(1,1) not null primary key, -- 기본키
 email varchar(50) not null, -- 인증 코드를 보낸 이메일
 code char(6) not null, -- 6자리 숫자 인증 코드 (D-032)
 expires_at datetime2(0) not null, -- 만료 시각 (UTC). 발송 시각 + 10분 (D-032)
 is_verified bit not null constraint df_email_verifications_is_verified default 0, -- 인증 완료 여부
 created_at datetime2(0) not null constraint df_email_verifications_created_at default sysutcdatetime(), -- 생성 시각 (UTC)
 updated_at datetime2(0) not null constraint df_email_verifications_updated_at default sysutcdatetime() -- 수정 시각 (UTC)
);
create index idx_email_verifications_email on email_verifications(email);
```
- 코드를 다시 보내면 새 행을 만든다. 확인과 가입에는 그 이메일의 가장 최근 행만 쓴다.

### 4.3 recipients (수신자)
```sql
create table recipients(
 id bigint identity(1,1) not null primary key, -- 기본키
 user_id bigint not null, -- 수신자 회원
 created_at datetime2(0) not null constraint df_recipients_created_at default sysutcdatetime(), -- 생성 시각 (UTC)
 updated_at datetime2(0) not null constraint df_recipients_updated_at default sysutcdatetime(), -- 수정 시각 (UTC)
 constraint fk_recipients_user_id foreign key (user_id) references users(id)
);
create unique index idx_recipients_user_id on recipients(user_id); -- 한 회원은 한 번만 수신자로 등록
```

### 4.4 newsletters (뉴스레터)
```sql
create table newsletters(
 id bigint identity(1,1) not null primary key, -- 기본키
 newsletter_date date not null, -- 뉴스레터 날짜 (KST). 생성(갱신)한 날
 title nvarchar(50) not null, -- 제목. "[뉴스레터] YYYY-MM-DD" (D-053)
 content_html nvarchar(max) null, -- HTML 본문. 생성 실패면 null
 status varchar(20) not null, -- 상태. Generated / GenerationFailed / Sent / SendFailed
 sent_at datetime2(0) null, -- 마지막 발송 시각 (UTC). 발송한 적 없으면 null
 created_at datetime2(0) not null constraint df_newsletters_created_at default sysutcdatetime(), -- 생성 시각 (UTC)
 updated_at datetime2(0) not null constraint df_newsletters_updated_at default sysutcdatetime(), -- 수정 시각 (UTC)
 constraint ck_newsletters_status check (status in ('Generated', 'GenerationFailed', 'Sent', 'SendFailed'))
);
create index idx_newsletters_status on newsletters(status);
```
- 상태 뜻
  - `Generated`: 생성됨, 미발송
  - `GenerationFailed`: 데이터를 받지 못해 생성 실패 (발송 페이지에 "생성 실패" 행으로 표시, D-042)
  - `Sent`: 모든 수신자에게 발송 성공 (히스토리에 보임)
  - `SendFailed`: 일부 또는 전체 발송 실패. 미발송으로 보아 다시 발송할 수 있고 히스토리에 보이지 않음 (D-014, D-040)

### 4.5 send_logs (발송 기록)
```sql
create table send_logs(
 id bigint identity(1,1) not null primary key, -- 기본키
 newsletter_id bigint not null, -- 발송한 뉴스레터
 user_id bigint not null, -- 받은 회원 (D-054: 이메일은 따로 저장하지 않음)
 is_success bit not null, -- 발송 성공 여부
 sent_at datetime2(0) not null, -- 발송 시도 시각 (UTC)
 created_at datetime2(0) not null constraint df_send_logs_created_at default sysutcdatetime(), -- 생성 시각 (UTC)
 updated_at datetime2(0) not null constraint df_send_logs_updated_at default sysutcdatetime(), -- 수정 시각 (UTC)
 constraint fk_send_logs_newsletter_id foreign key (newsletter_id) references newsletters(id),
 constraint fk_send_logs_user_id foreign key (user_id) references users(id)
);
create index idx_send_logs_newsletter_id on send_logs(newsletter_id);
```
- 발송할 때마다 수신자별로 한 행씩 쌓는다. 수신자를 삭제해도 남는다(D-055).
- "보내진 이메일"(D-039)은 `is_success = 1`인 행의 `user_id`로 users의 이메일을 조회해 표시한다.

### 4.6 newsletter_settings (뉴스레터 설정)
```sql
create table newsletter_settings(
 id bigint identity(1,1) not null primary key, -- 기본키
 generate_time time(0) not null constraint df_newsletter_settings_generate_time default '07:50', -- 자동 생성 시각 (KST, 분 단위) (D-043)
 send_time time(0) not null constraint df_newsletter_settings_send_time default '08:00', -- 자동 발송 시각 (KST, 분 단위) (D-043)
 last_generate_date date null, -- 자동 생성을 마지막으로 실행한 날짜 (KST). 같은 날 중복 실행 방지
 last_send_date date null, -- 자동 발송을 마지막으로 실행한 날짜 (KST). 같은 날 중복 실행 방지
 created_at datetime2(0) not null constraint df_newsletter_settings_created_at default sysutcdatetime(), -- 생성 시각 (UTC)
 updated_at datetime2(0) not null constraint df_newsletter_settings_updated_at default sysutcdatetime(), -- 수정 시각 (UTC)
 constraint ck_newsletter_settings_time check (generate_time < send_time) -- 생성 시간은 발송 시간보다 앞서야 함 (D-043)
);
insert into newsletter_settings (generate_time, send_time) values ('07:50', '08:00'); -- 설정은 이 1행만 쓴다
```

### 4.7 holidays (공휴일)
```sql
create table holidays(
 id bigint identity(1,1) not null primary key, -- 기본키
 holiday_date date not null, -- 공휴일, 대체공휴일 날짜
 name nvarchar(50) not null, -- 공휴일 이름 (특일정보 API의 dateName)
 created_at datetime2(0) not null constraint df_holidays_created_at default sysutcdatetime(), -- 생성 시각 (UTC)
 updated_at datetime2(0) not null constraint df_holidays_updated_at default sysutcdatetime() -- 수정 시각 (UTC)
);
create unique index idx_holidays_holiday_date on holidays(holiday_date);
```
- 같은 날짜에 공휴일이 둘 이상이면 한 행만 저장한다(이름은 먼저 받은 것). 발송 제외 판별에는 날짜만 쓴다.
