pipeline {
    agent any

    environment {
        DOCKER_HUB_USER = 'mtaidev'
        IMAGE_NAME      = 'notaion-backend'
        IMAGE_TAG       = "${BUILD_NUMBER}"
        CONTAINER_NAME  = 'notaion-backend'
        APP_PORT        = '8081'
        FTP_HOST        = 'site8642.siteasp.net'
        FTP_REMOTE_DIR  = '/wwwroot'
        PUBLISH_DIR     = "${WORKSPACE}/publish_output"
        DOTNET_CLI_TELEMETRY_OPTOUT = '1'
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

       stage('Build & Publish') {
    steps {
        echo '🔨 Đang dotnet publish qua Docker...'
        sh """
            rm -rf /var/jenkins_home/workspace/notaion-backend/publish_output
            docker run --rm \
                -v /var/jenkins_home/workspace/notaion-backend:/src \
                -v /var/jenkins_home/workspace/notaion-backend/publish_output:/publish \
                -w /src/NotaionWebApp/Notaion \
                mcr.microsoft.com/dotnet/sdk:9.0 \
                dotnet publish Notaion.csproj -c Release -o /publish --self-contained false
        """
    }
}

        stage('Write web.config') {
            steps {
                echo '📝 Ghi web.config chuẩn IIS...'
                sh """
                    cat > ${PUBLISH_DIR}/web.config <<'WEBCONFIG'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*"
             modules="AspNetCoreModuleV2"
             resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet"
                  arguments=".\\Notaion.dll"
                  stdoutLogEnabled="false"
                  stdoutLogPath=".\\logs\\stdout"
                  hostingModel="inprocess" />
    </system.webServer>
  </location>
</configuration>
WEBCONFIG
                """
            }
        }

        stage('Deploy to MonsterASP (FTP)') {
            steps {
                echo '🌐 Đang deploy lên MonsterASP qua FTP...'
                withCredentials([usernamePassword(
                    credentialsId: 'monsterasp-ftp-creds',
                    usernameVariable: 'FTP_USER',
                    passwordVariable: 'FTP_PASS'
                )]) {
                    sh """
                        set -e

                        cat > ${PUBLISH_DIR}/app_offline.htm <<'OFFLINE'
<!doctype html><html><body><h1>Deploying...</h1></body></html>
OFFLINE

                        docker run --rm \
                            -v ${PUBLISH_DIR}:/data \
                            -e FTP_USER="\$FTP_USER" \
                            -e FTP_PASS="\$FTP_PASS" \
                            -e FTP_HOST="${FTP_HOST}" \
                            -e FTP_REMOTE_DIR="${FTP_REMOTE_DIR}" \
                            alpine:3.20 sh -c '
                                apk add --no-cache lftp >/dev/null 2>&1

                                lftp -u "\$FTP_USER","\$FTP_PASS" "ftp://\$FTP_HOST" <<LFTP
set ssl:verify-certificate no
set ftp:ssl-allow yes
set ftp:ssl-protect-data no
set ftp:passive-mode on
set ftp:prefer-epsv no
set ftp:use-site-chmod no
set ftp:use-site-utime no
set ftp:use-site-utime2 no
set mirror:set-permissions no
set mirror:parallel-transfer-count 1
set net:max-retries 10
set net:reconnect-interval-base 3
set net:reconnect-interval-multiplier 1
set net:timeout 60
set xfer:clobber yes
set xfer:timeout 120
set xfer:use-temp-file no
mkdir -p \$FTP_REMOTE_DIR
put -O \$FTP_REMOTE_DIR /data/app_offline.htm
mirror -R --delete --continue --verbose --no-perms --exclude-glob app_offline.htm /data \$FTP_REMOTE_DIR
rm -f \$FTP_REMOTE_DIR/app_offline.htm
bye
LFTP
                            '

                        echo "✅ Deploy FTP hoàn tất."
                    """
                }
            }
        }

        stage('Build & Run Docker (local)') {
            steps {
                echo '🐳 Build và chạy container local...'
                sh """
                    docker build -t ${DOCKER_HUB_USER}/${IMAGE_NAME}:${IMAGE_TAG} .
                    docker tag  ${DOCKER_HUB_USER}/${IMAGE_NAME}:${IMAGE_TAG} \
                                ${DOCKER_HUB_USER}/${IMAGE_NAME}:latest

                    docker stop ${CONTAINER_NAME} || true
                    docker rm   ${CONTAINER_NAME} || true
                    docker run -d \
                        --name ${CONTAINER_NAME} \
                        --restart always \
                        -p ${APP_PORT}:8080 \
                        -e BUILD_NUMBER=${BUILD_NUMBER} \
                        -e APP_VERSION=${IMAGE_TAG} \
                        ${DOCKER_HUB_USER}/${IMAGE_NAME}:latest
                """
            }
        }
    }

    post {
        success {
            echo "✅ Deploy thành công! Build #${BUILD_NUMBER}"
            echo "🌐 Local:      http://localhost:${APP_PORT}"
            echo "🌐 MonsterASP: http://notaion.runasp.net"
        }
        failure {
            echo "❌ Deploy thất bại tại Build #${BUILD_NUMBER} - Kiểm tra log!"
        }
        always {
            echo '🧹 Dọn dẹp...'
            sh """
                rm -rf ${PUBLISH_DIR}
                docker image prune -f
            """
        }
    }
}
