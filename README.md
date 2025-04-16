# BackendFrontend

## 개요
* 빠른 송수신이 필요한 멀티게임을 만들기 위해 개발
* 서버 구축에 필요한 비용을 최대한 절감하기 위해 P2P 방식 적용
* TCP Server가 실행되는 컴퓨터만 사용하여, 반응속도가 빠른 멀티 게임을 만들기 위함

## 핵심 기능
### Server
+ TCP Server
  + TCP Client 연결 및 매칭
  + 메세지 송수신
    
+ Stun Server(UDP 통신)
  + UDP Hole Punching

**TCP, UDP 포트포워딩 필요**

### Client
+ TCP Client
  + TCP Server에 접속
  + 매칭 신청 및 취소

* UDP Client(Host 방식 P2P)
  + Host Client
    + Clients의 입력 값 수신
    + 수신한 입력 값 계산 및 적용
    + 적용된 Position을 Clients에게 전달
      
  * Client
    + Host에게 입력 값 전달
    + Host로 부터 수신한 좌표 값 적용

### MongoDB Community
+ MongoDBCommunity
  + TCP Server와 송수신
  + ObjectID, ID, Password, Nickname 저장
  
## 장단점
### 장점
* TCP Server가 구동되는 컴퓨터 1대만 필요(추 후 확장이 필요할 시, 증가)
* UDP P2P 방식을 통해 빠른 반응이 필요한 게임(FPS 등...)제작 가능
* Host Client를 제외한 나머지 Client들의 핵 방지 가능
  
### 단점
* 1개의 게임에 들어가는 인원이 많아 질 수록 Host Client의 성능 중요
* Host Client가 핵을 사용하는 경우 막지 못함

## 추가 및 수정 작업 List
### Server
* TCP Client들의 연결 상태를 확인 하기 위해, Ping Pong 메세지를 주고 받는 기능 추가 (HeartBeat)
* DB 연동 후, Clients Data 관리

### Client
* 재접속 기능 추가
* InGame 내의 Host Client 끊김 및 변경 처리 기능 추가
* InGame 시작 및 결과 데이터 송수신 

