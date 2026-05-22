# 🚀 Jenkins + Docker + MonsterASP — Hướng dẫn CI/CD

> Cập nhật lần cuối: 21/05/2026 — Project: notaion-backend (.NET 9)

---

## 📋 Mục lục

1. [Kiến trúc tổng quan](#kiến-trúc-tổng-quan)
2. [Yêu cầu](#yêu-cầu)
3. [Cài đặt Jenkins bằng Docker Compose](#cài-đặt-jenkins-bằng-docker-compose)
4. [Cấu hình Jenkins lần đầu](#cấu-hình-jenkins-lần-đầu)
5. [Dockerfile cho .NET 9](#dockerfile-cho-net-9)
6. [Jenkinsfile hoàn chỉnh](#jenkinsfile-hoàn-chỉnh)
7. [Credentials trong Jenkins](#credentials-trong-jenkins)
8. [API kiểm tra deploy](#api-kiểm-tra-deploy)
9. [Lệnh Docker hữu ích](#lệnh-docker-hữu-ích)
10. [Troubleshooting](#troubleshooting)

---

## Kiến trúc tổng quan

```
git push origin main
        ↓
GitHub gửi Webhook → Jenkins
        ↓
Jenkins clone code mới
        ↓
Build Docker Image (.NET 9)
        ↓
Deploy lên Docker Hub (mtaidev/notaion-backend)
        ↓
Deploy container trên máy local (port 8081)
        ↓
Deploy lên MonsterASP qua WebDeploy (notaion.runasp.net)
        ↓
App chạy phiên bản mới nhất ✅
```

---

## Yêu cầu

| Công cụ | Phiên bản |
|---|---|
| Docker Desktop | 24.x trở lên |
| Docker Compose | v2.x trở lên |
| PowerShell | 7.x (pwsh) |

```powershell
docker --version
docker compose version
```

---

## Cài đặt Jenkins bằng Docker Compose

### Bước 1 — Tạo thư mục và file compose

```powershell
mkdir C:\jenkins-docker
cd C:\jenkins-docker
notepad docker-compose.yml
```

Nội dung `docker-compose.yml`:

```yaml
version: '3.8'

services:
  jenkins:
    image: jenkins/jenkins:lts
    container_name: jenkins
    restart: always
    ports:
      - "8080:8080"
      - "50000:50000"
    volumes:
      - jenkins_home:/var/jenkins_home
      - /var/run/docker.sock:/var/run/docker.sock

volumes:
  jenkins_home:
```

### Bước 2 — Khởi chạy Jenkins

```powershell
docker compose up -d
```

### Bước 3 — Lấy mật khẩu admin lần đầu

```powershell
docker exec jenkins cat /var/jenkins_home/secrets/initialAdminPassword
```

### Bước 4 — Cài Docker CLI và zip vào Jenkins container

```powershell
docker exec -u root jenkins bash -c "apt-get update && apt-get install -y docker.io zip"
docker exec -u root jenkins chmod 666 /var/run/docker.sock
```

### Bước 5 — Truy cập Jenkins

Mở trình duyệt: **http://localhost:8080**

---

## Cấu hình Jenkins lần đầu

1. Nhập mật khẩu admin từ bước 3
2. Chọn **"Install suggested plugins"**
3. Tạo tài khoản admin
4. Cài thêm plugins:

```
Manage Jenkins → Plugins → Available plugins

✅ GitHub Integration Plugin
✅ Docker Pipeline
✅ SSH Agent Plugin
```

---

## Dockerfile cho .NET 9

Tạo file `Dockerfile` ở **root repo** (cùng cấp Jenkinsfile):

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
WORKDIR "/src/NotaionWebApp/Notaion"
RUN dotnet restore "Notaion.csproj"
RUN dotnet build "Notaion.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR "/src/NotaionWebApp/Notaion"
RUN dotnet publish "Notaion.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notaion.dll"]
```

---

## Jenkinsfile hoàn chỉnh

Tạo/thay file `Jenkinsfile` ở **root repo**:

```groovy
pipeline {
    agent any

    environment {
        DOCKER_HUB_USER = 'mtaidev'
        IMAGE_NAME      = 'notaion-backend'
        IMAGE_TAG       = "${BUILD_NUMBER}"
        CONTAINER_NAME  = 'notaion-backend'
        APP_PORT        = '8081'
    }

    triggers {
        githubPush()
    }

    stages {

        stage('Cloning') {
            steps {
                echo '📥 Đang clone source code...'
                git branch: 'main',
                    credentialsId: '3ceb09c4-d257-4d6b-b65c-26db994addff',
                    url: 'https://github.com/mtai0524/notaion-backend.git'
            }
        }

        stage('Build Docker Image') {
            steps {
                echo '🐳 Đang build Docker image...'
                sh """
                    docker build -t ${DOCKER_HUB_USER}/${IMAGE_NAME}:${IMAGE_TAG} .
                    docker tag ${DOCKER_HUB_USER}/${IMAGE_NAME}:${IMAGE_TAG} \
                               ${DOCKER_HUB_USER}/${IMAGE_NAME}:latest
                """
            }
        }

        stage('Push to Docker Hub') {
            steps {
                echo '📤 Đang push image lên Docker Hub...'
                withCredentials([usernamePassword(
                    credentialsId: 'dockerhub-creds',
                    usernameVariable: 'DOCKER_USER',
                    passwordVariable: 'DOCKER_PASS'
                )]) {
                    sh """
                        echo \$DOCKER_PASS | docker login -u \$DOCKER_USER --password-stdin
                        docker push ${DOCKER_HUB_USER}/${IMAGE_NAME}:${IMAGE_TAG}
                        docker push ${DOCKER_HUB_USER}/${IMAGE_NAME}:latest
                    """
                }
            }
        }

        stage('Deploy to MonsterASP') {
            steps {
                echo '🌐 Đang deploy lên MonsterASP...'
                withCredentials([usernamePassword(
                    credentialsId: 'monsterasp-creds',
                    usernameVariable: 'DEPLOY_USER',
                    passwordVariable: 'DEPLOY_PASS'
                )]) {
                    sh '''
                        docker create --name temp_extract mtaidev/notaion-backend:latest
                        docker cp temp_extract:/app ./publish_output
                        docker rm temp_extract
                        cd publish_output && zip -r ../deploy.zip . && cd ..
                        curl -k -X POST \
                            "https://site8642.siteasp.net:8172/msdeploy.axd?site=site8642" \
                            -u "$DEPLOY_USER:$DEPLOY_PASS" \
                            --data-binary @deploy.zip \
                            -H "Content-Type: application/zip"
                        rm -rf publish_output deploy.zip
                    '''
                }
            }
        }

        stage('Deploy Container') {
            steps {
                echo '🚀 Đang deploy container local...'
                sh """
                    docker stop ${CONTAINER_NAME} || true
                    docker rm   ${CONTAINER_NAME} || true
                    docker pull ${DOCKER_HUB_USER}/${IMAGE_NAME}:latest
                    docker run -d \
                        --name ${CONTAINER_NAME} \
                        --restart always \
                        -p ${APP_PORT}:8080 \
                        -e BUILD_NUMBER=${BUILD_NUMBER} \
                        -e "DEPLOY_TIME=\$(date '+%Y-%m-%d %H:%M:%S')" \
                        -e APP_VERSION=${IMAGE_TAG} \
                        ${DOCKER_HUB_USER}/${IMAGE_NAME}:latest
                """
            }
        }
    }

    post {
        success {
            echo "✅ Deploy thành công! Build #${BUILD_NUMBER}"
            echo "🌐 Local: http://localhost:${APP_PORT}"
            echo "🌐 MonsterASP: http://notaion.runasp.net"
        }
        failure {
            echo "❌ Deploy thất bại tại Build #${BUILD_NUMBER} - Kiểm tra log!"
        }
        always {
            echo '🧹 Dọn dẹp images cũ...'
            sh "docker image prune -f"
        }
    }
}
```

---

## Credentials trong Jenkins

```
Manage Jenkins → Credentials → System → Global credentials → Add Credentials
```

| ID | Kind | Username | Dùng cho |
|---|---|---|---|
| `dockerhub-creds` | Username/Password | `mtaidev` | Push Docker Hub |
| `monsterasp-creds` | Username/Password | `site8642` | Deploy MonsterASP |

---

## API kiểm tra deploy

Thêm `DeployInfoController.cs` vào project:

```csharp
[ApiController]
[Route("api/[controller]")]
public class DeployInfoController : ControllerBase
{
    [HttpGet("info")]
    public IActionResult GetDeployInfo()
    {
        return Ok(new
        {
            status      = "✅ Running",
            deployedAt  = Environment.GetEnvironmentVariable("DEPLOY_TIME") ?? "unknown",
            buildNumber = Environment.GetEnvironmentVariable("BUILD_NUMBER") ?? "unknown",
            version     = Environment.GetEnvironmentVariable("APP_VERSION") ?? "1.0.0",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        });
    }
}
```

Gọi API kiểm tra:
```
http://localhost:8081/api/DeployInfo/info
http://notaion.runasp.net/api/DeployInfo/info
```

Kết quả mẫu:
```json
{
  "status": "✅ Running",
  "deployedAt": "2026-05-21 15:30:00",
  "buildNumber": "11",
  "version": "11",
  "environment": "Production"
}
```

---

## Lệnh Docker hữu ích

```powershell
# Khởi động Jenkins
docker compose up -d

# Dừng Jenkins
docker compose down

# Xem log Jenkins
docker logs -f jenkins

# Xem log app
docker logs -f notaion-backend

# Xem trạng thái container
docker ps

# Vào trong container Jenkins (cài thêm tool)
docker exec -u root -it jenkins bash

# Cài zip và docker cli vào Jenkins (chạy 1 lần)
docker exec -u root jenkins bash -c "apt-get update && apt-get install -y docker.io zip"
docker exec -u root jenkins chmod 666 /var/run/docker.sock

# Dọn dẹp images không dùng
docker image prune -f

# Dọn dẹp toàn bộ
docker system prune -f
```

---

## Troubleshooting

### Jenkins không tìm thấy docker
```powershell
docker exec -u root jenkins bash -c "apt-get update && apt-get install -y docker.io"
docker exec -u root jenkins chmod 666 /var/run/docker.sock
```

### Jenkins không tìm thấy zip
```powershell
docker exec -u root jenkins apt-get install -y zip
```

### App không truy cập được
```powershell
# Kiểm tra port app đang lắng nghe
docker logs notaion-backend

# Nếu app lắng nghe 8080, map đúng port
docker run -p 8081:8080 mtaidev/notaion-backend:latest
```

### DEPLOY_TIME có dấu cách làm vỡ lệnh docker run
```groovy
// Dùng dấu ngoặc kép bao quanh giá trị -e
-e "DEPLOY_TIME=\$(date '+%Y-%m-%d %H:%M:%S')"
```

### Reset mật khẩu admin Jenkins
```powershell
docker exec jenkins cat /var/jenkins_home/secrets/initialAdminPassword
```

---

*Tài liệu được cập nhật theo tiến trình setup thực tế — notaion-backend project.*
