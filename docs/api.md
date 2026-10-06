# api.md

API의 기준 문서다. API를 추가하거나 바꾸는 작업에서는 이 문서도 함께 갱신한다.

이 문서와 실제 API가 다르면 사용자에게 알린다.

---

## 1. 개요

- **기본 경로**: /api
- **데이터 형식**: JSON (UTF-8)
- **인증 방식**: ASP.NET Core 쿠키 인증 (로그인하면 인증 쿠키 발급, 로그아웃하면 삭제)
  - 쿠키: HttpOnly, SameSite=Strict, 만료 60분(요청이 있으면 연장)
  - CSRF: 모든 POST 요청은 `RequestVerificationToken` 헤더에 Razor 뷰가 발급한 안티포저리 토큰을 담아 보낸다
  - 로그인하지 않은 요청은 401 UNAUTHORIZED, 권한이 없는 요청은 403 FORBIDDEN을 JSON으로 응답한다 (로그인 화면으로 리다이렉트하지 않음)
- **날짜 형식**
  - 시각: ISO 8601, UTC (2026-01-01T00:00:00Z). 화면에서 KST로 바꿔 표시한다
  - 날짜: `YYYY-MM-DD` (KST)
  - 시간(뉴스레터 설정): `HH:mm` (KST)
- **화면 경로**: 화면(Razor 뷰)은 /api가 아닌 화면 컨트롤러 경로로 연다. 화면은 틀만 그리고 데이터는 아래 API로 받는다 (D-057)

---

## 2. 네이밍 규칙

| 대상 | 규칙 | 예 |
|---|---|---|
| 경로 | 소문자 kebab-case, 복수형 명사 | `/api/users` |
| 경로 변수 | `{id}` | `/api/users/{id}` |
| 요청/응답 필드 | camelCase | `userName`, `createdAt` |
| 쿼리 파라미터 | camelCase | `?pageSize=20` |

### 메서드
| 메서드 | 용도 |
|---|---|
| GET | 조회 |
| POST | 생성/수정/삭제 |

---

## 3. 공통 응답 형식

### 성공
```json
{
  "data": {}
}
```

### 실패
```json
{
  "error": {
    "code": "USER_NOT_FOUND",
    "message": "사용자를 찾을 수 없습니다."
  }
}
```

---

## 4. 오류 코드

| HTTP 상태 | 코드 | 설명 |
|---|---|---|
| 400 | INVALID_INPUT | 입력값이 올바르지 않음 |
| 401 | UNAUTHORIZED | 로그인이 필요함 |
| 403 | FORBIDDEN | 권한이 없음 |
| 404 | NOT_FOUND | 대상이 없음 |
| 500 | INTERNAL_ERROR | 서버 오류 |

<!-- 프로젝트 전용 오류 코드는 이 아래에 추가한다 -->
| 400 | VERIFICATION_CODE_INVALID | 인증 코드가 맞지 않음 |
| 400 | VERIFICATION_CODE_EXPIRED | 인증 코드 유효 시간(10분)이 지남 |
| 400 | EMAIL_NOT_VERIFIED | 이메일 인증을 하지 않음 |
| 400 | INVALID_SETTING_TIME | 생성 시간이 발송 시간보다 앞서지 않음 |
| 400 | NO_RECIPIENTS | 발송할 수신자가 없음 |
| 401 | LOGIN_FAILED | 아이디 또는 비밀번호가 맞지 않음 (어느 쪽이 틀렸는지 알리지 않음) |
| 409 | LOGIN_ID_DUPLICATED | 이미 사용 중인 아이디 |
| 409 | EMAIL_DUPLICATED | 이미 사용 중인 이메일 |
| 409 | ALREADY_RECIPIENT | 이미 수신자로 등록된 회원 |
| 409 | NEWSLETTER_NOT_SENDABLE | 발송할 수 없는 뉴스레터 (이미 발송됨 또는 생성 실패) |
| 500 | MAIL_SEND_FAILED | 메일 발송 실패 (인증 코드 메일) |

---

## 5. API 정의

<!-- API마다 아래 형식을 복사해서 쓴다 -->
### {{메서드}} {{경로}}
- **설명**: 
- **권한**: (overview.md "사용자와 권한" 기준)
- **요청**:
  | 필드 | 타입 | 필수 | 설명 |
  |---|---|---|---|
- **응답 data**:
  | 필드 | 타입 | 설명 |
  |---|---|---|
- **오류**: (4. 오류 코드 중 발생할 수 있는 것)
- **사용 테이블**: (database.md 기준)

---

## 6. 실제 사용할 API
<!-- 실제 API 정의를 이 아래에 작성 -->

모든 API는 서버에서 로그인 여부와 권한을 확인한다. 모든 요청에는 400 INVALID_INPUT, 500 INTERNAL_ERROR가 발생할 수 있어 각 API의 "오류"에서는 생략한다.

### 6.1 회원가입, 로그인

#### POST /api/email-verifications
- **설명**: 이메일로 6자리 숫자 인증 코드를 보낸다. 유효 시간은 10분이다. 다시 요청하면 새 코드를 보내고 이전 코드는 쓰지 않는다.
- **권한**: 비회원
- **요청**:
  | 필드 | 타입 | 필수 | 설명 |
  |---|---|---|---|
  | email | string | O | 기본 이메일 형식, 최대 50자 |
- **응답 data**:
  | 필드 | 타입 | 설명 |
  |---|---|---|
  | expiresAt | string | 코드 만료 시각 (UTC) |
- **오류**: EMAIL_DUPLICATED, MAIL_SEND_FAILED
- **사용 테이블**: users, email_verifications

#### POST /api/email-verifications/confirm
- **설명**: 그 이메일로 가장 최근에 보낸 인증 코드를 확인한다.
- **권한**: 비회원
- **요청**:
  | 필드 | 타입 | 필수 | 설명 |
  |---|---|---|---|
  | email | string | O | 코드를 받은 이메일 |
  | code | string | O | 6자리 숫자 |
- **응답 data**:
  | 필드 | 타입 | 설명 |
  |---|---|---|
  | isVerified | boolean | 인증 성공이면 true |
- **오류**: VERIFICATION_CODE_INVALID, VERIFICATION_CODE_EXPIRED, NOT_FOUND(보낸 코드가 없음)
- **사용 테이블**: email_verifications

#### POST /api/users
- **설명**: 회원가입. 이메일의 가장 최근 인증 행이 인증 완료 상태여야 한다. 가입한 회원의 권한은 User다.
- **권한**: 비회원
- **요청**:
  | 필드 | 타입 | 필수 | 설명 |
  |---|---|---|---|
  | loginId | string | O | 영문 소문자+숫자 6~20자 |
  | password | string | O | 8~20자, 영문·숫자·특수문자(공백 제외 ASCII 특수문자 32개) 각 1자 이상 |
  | email | string | O | 기본 이메일 형식, 최대 50자 |
  | name | string | O | 2~30자 |
- **응답 data**:
  | 필드 | 타입 | 설명 |
  |---|---|---|
  | id | number | 가입한 회원 id |
- **오류**: LOGIN_ID_DUPLICATED, EMAIL_DUPLICATED, EMAIL_NOT_VERIFIED
- **사용 테이블**: users, email_verifications

#### POST /api/auth/login
- **설명**: 로그인하고 인증 쿠키를 발급한다.
- **권한**: 비회원
- **요청**:
  | 필드 | 타입 | 필수 | 설명 |
  |---|---|---|---|
  | loginId | string | O | 아이디 |
  | password | string | O | 비밀번호 |
- **응답 data**:
  | 필드 | 타입 | 설명 |
  |---|---|---|
  | role | string | User / Admin |
  | redirectUrl | string | 로그인 후 이동할 화면. User: 히스토리 목록, Admin: 뉴스레터 발송 (D-048) |
- **오류**: LOGIN_FAILED
- **사용 테이블**: users

#### POST /api/auth/logout
- **설명**: 로그아웃하고 인증 쿠키를 삭제한다.
- **권한**: 일반 사용자, 관리자
- **요청**: 없음
- **응답 data**: 없음 (`{}`)
- **오류**: UNAUTHORIZED
- **사용 테이블**: 없음

### 6.2 수신자 관리

#### GET /api/users
- **설명**: 가입한 회원 전체 목록. 수신자 여부를 함께 준다 (수신자 추가 대상 선택용).
- **권한**: 관리자
- **요청**: 없음
- **응답 data**: 배열
  | 필드 | 타입 | 설명 |
  |---|---|---|
  | id | number | 회원 id |
  | loginId | string | 아이디 |
  | name | string | 이름 |
  | email | string | 이메일 |
  | role | string | User / Admin |
  | isRecipient | boolean | 수신자 여부 |
- **오류**: UNAUTHORIZED, FORBIDDEN
- **사용 테이블**: users, recipients

#### GET /api/recipients
- **설명**: 수신자 목록 (수신자 관리 화면, 발송 화면의 수신자 목록)
- **권한**: 관리자
- **요청**: 없음
- **응답 data**: 배열
  | 필드 | 타입 | 설명 |
  |---|---|---|
  | userId | number | 회원 id |
  | loginId | string | 아이디 |
  | name | string | 이름 |
  | email | string | 이메일 |
  | createdAt | string | 수신자 등록 시각 (UTC) |
- **오류**: UNAUTHORIZED, FORBIDDEN
- **사용 테이블**: recipients, users

#### POST /api/recipients
- **설명**: 회원을 수신자로 추가한다.
- **권한**: 관리자
- **요청**:
  | 필드 | 타입 | 필수 | 설명 |
  |---|---|---|---|
  | userId | number | O | 회원 id |
- **응답 data**: 없음 (`{}`)
- **오류**: UNAUTHORIZED, FORBIDDEN, NOT_FOUND(회원 없음), ALREADY_RECIPIENT
- **사용 테이블**: recipients, users

#### POST /api/recipients/{userId}/delete
- **설명**: 수신자에서 삭제한다. 발송 기록은 남는다 (D-055).
- **권한**: 관리자
- **요청**: 없음 (경로 변수 userId)
- **응답 data**: 없음 (`{}`)
- **오류**: UNAUTHORIZED, FORBIDDEN, NOT_FOUND(수신자 아님)
- **사용 테이블**: recipients

### 6.3 뉴스레터 설정 관리

#### GET /api/newsletter-settings
- **설명**: 자동 생성·발송 시간 조회
- **권한**: 관리자
- **요청**: 없음
- **응답 data**:
  | 필드 | 타입 | 설명 |
  |---|---|---|
  | generateTime | string | 자동 생성 시간 `HH:mm` (KST) |
  | sendTime | string | 자동 발송 시간 `HH:mm` (KST) |
- **오류**: UNAUTHORIZED, FORBIDDEN
- **사용 테이블**: newsletter_settings

#### POST /api/newsletter-settings
- **설명**: 자동 생성·발송 시간 저장. 생성 시간은 발송 시간보다 앞서야 한다 (D-043).
- **권한**: 관리자
- **요청**:
  | 필드 | 타입 | 필수 | 설명 |
  |---|---|---|---|
  | generateTime | string | O | `HH:mm` (00:00~23:59) |
  | sendTime | string | O | `HH:mm` (00:00~23:59) |
- **응답 data**: GET과 같음
- **오류**: UNAUTHORIZED, FORBIDDEN, INVALID_SETTING_TIME
- **사용 테이블**: newsletter_settings

### 6.4 뉴스레터 직접 생성/발송

#### GET /api/newsletters
- **설명**: 최신 뉴스레터 10개 (발송 화면 목록). 생성 실패 행과 보내진 이메일을 포함한다 (D-035, D-039, D-042).
- **권한**: 관리자
- **요청**: 없음
- **응답 data**: 배열 (최근 생성 순)
  | 필드 | 타입 | 설명 |
  |---|---|---|
  | id | number | 뉴스레터 id |
  | title | string | 제목 |
  | status | string | Generated / GenerationFailed / Sent / SendFailed |
  | isSendable | boolean | 발송할 수 있으면 true (Generated, SendFailed) |
  | createdAt | string | 생성 시각 (UTC) |
  | sentAt | string \| null | 마지막 발송 시각 (UTC) |
  | sentEmails | string[] | 발송에 성공한 이메일 목록 |
- **오류**: UNAUTHORIZED, FORBIDDEN
- **사용 테이블**: newsletters, send_logs, users

#### POST /api/newsletters/generate
- **설명**: 뉴스레터 생성. 미발송 뉴스레터가 있으면 갱신하고, 없으면 새로 만든다 (D-028). 데이터를 받지 못하면 생성 실패 상태로 남긴다. 석유 데이터는 출처가 정해질 때까지 임시 데이터를 쓴다 (D-050).
- **권한**: 관리자
- **요청**: 없음
- **응답 data**:
  | 필드 | 타입 | 설명 |
  |---|---|---|
  | id | number | 뉴스레터 id |
  | status | string | Generated / GenerationFailed |
- **오류**: UNAUTHORIZED, FORBIDDEN
- **사용 테이블**: newsletters

#### POST /api/newsletters/{id}/send
- **설명**: 뉴스레터 발송. 수신자를 선택하지 않으면 전체 수신자에게 보낸다 (D-041). 한 명이라도 실패하면 SendFailed다 (D-014).
- **권한**: 관리자
- **요청**:
  | 필드 | 타입 | 필수 | 설명 |
  |---|---|---|---|
  | userIds | number[] | X | 보낼 수신자 회원 id. 비었거나 없으면 전체 수신자 |
- **응답 data**:
  | 필드 | 타입 | 설명 |
  |---|---|---|
  | status | string | Sent / SendFailed |
  | successCount | number | 성공한 수신자 수 |
  | failCount | number | 실패한 수신자 수 |
- **오류**: UNAUTHORIZED, FORBIDDEN, NOT_FOUND(뉴스레터 없음 또는 수신자가 아닌 userId 포함), NEWSLETTER_NOT_SENDABLE, NO_RECIPIENTS
- **사용 테이블**: newsletters, recipients, users, send_logs

### 6.5 뉴스레터 히스토리 (보낸 메일 화면, D-063)

#### GET /api/histories
- **설명**: 발송 성공(Sent)한 뉴스레터를 최근 발송 순으로 30개씩 페이징 (D-018, D-038, D-040)
- **권한**: 일반 사용자, 관리자
- **요청 (쿼리)**:
  | 필드 | 타입 | 필수 | 설명 |
  |---|---|---|---|
  | page | number | X | 1부터 시작. 기본 1 |
- **응답 data**:
  | 필드 | 타입 | 설명 |
  |---|---|---|
  | items | 배열 | id(number), title(string), sentAt(string) |
  | page | number | 현재 페이지 |
  | totalPages | number | 전체 페이지 수 |
- **오류**: UNAUTHORIZED
- **사용 테이블**: newsletters

#### GET /api/histories/{id}
- **설명**: 히스토리 본문. 발송 성공(Sent)한 뉴스레터만 볼 수 있다.
- **권한**: 일반 사용자, 관리자
- **요청**: 없음 (경로 변수 id)
- **응답 data**:
  | 필드 | 타입 | 설명 |
  |---|---|---|
  | id | number | 뉴스레터 id |
  | title | string | 제목 |
  | contentHtml | string | HTML 본문 (화면에서는 sandbox iframe으로 표시) |
  | sentAt | string | 발송 시각 (UTC) |
- **오류**: UNAUTHORIZED, NOT_FOUND(없거나 Sent가 아닌 뉴스레터)
- **사용 테이블**: newsletters
