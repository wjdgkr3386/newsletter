# spec.md

기술 스택, 의존성을 정리한다.

---

## 1. 스택
|구분|선택|버전|
|---|---|---|
|프레임워크|ASP.NET Core MVC|.NET 10 (SDK 10.0.201)|
|자동 실행|ASP.NET Core BackgroundService (매분 설정 시간 확인, 추가 라이브러리 없음)| |
|공휴일 데이터|공공데이터포털 한국천문연구원 특일정보 API| |
|실행 환경|로컬| |
|언어|C#, HTML| |
|스타일링|부트스트랩 (뉴스레터 본문은 HTML + 기본 CSS)| |
|DB|MS-SQL (SQL Server Developer 에디션, 사용자가 직접 설치)| |
|ORM|EF Core (SQL Server 공급자)|10|
|인증|ASP.NET Core 쿠키 인증 (Identity 전체는 쓰지 않음)|.NET 10 기본 포함|
|비밀번호 해시|BCrypt| |
|메일 발송|Gmail SMTP (MailKit)| |
|비밀 정보 관리|.NET User Secrets (Gmail 앱 비밀번호, 특일정보 API 키, DB 접속 정보)|.NET 10 기본 포함|
|화면|Razor 뷰(화면 틀) + JS fetch로 JSON API 호출| |
|개발 도구|Visual Studio| |
|테스트 도구|xUnit v3| |


---

## 2. 의존성
버전은 2026-10-06 NuGet 최신 안정 버전 기준이다. 프로젝트를 만들 때 .NET 10 지원 여부와 `dotnet list package --vulnerable` 결과를 확인하고, 다르면 이 표를 갱신한다.

|이름|버전|용도|
|---|---|---|
|Microsoft.EntityFrameworkCore.SqlServer|10.0.12|EF Core SQL Server 공급자|
|BCrypt.Net-Next|4.2.1|비밀번호 단방향 해시|
|MailKit|4.18.1|Gmail SMTP 메일 발송|
|xunit.v3|4.0.1|테스트 프레임워크 (tests/Newsletter.Tests)|
|xunit.runner.visualstudio|4.0.0|테스트 실행기 (tests/Newsletter.Tests)|
|Microsoft.NET.Test.Sdk|18.10.1|테스트 실행 SDK (tests/Newsletter.Tests)|
|Bootstrap|5.x (MVC 템플릿 기본 포함 버전)|화면 스타일|

---
