# database.md

데이터 모델의 기준 문서다. 스키마를 바꾸는 작업에서는 이 문서도 함께 갱신한다.

이 문서와 실제 스키마가 다르면 사용자에게 알린다.

---

## 1. 개요

- **DB**: {{예: MySQL 8.4}}

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
- `id`: {{bigint auto_increment}}
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
