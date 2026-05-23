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
        PUBLISH_DIR     = '/var/jenkins_home/workspace/notaion-backend/publish_output'
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

        stage('Extract Publish Output') {
            steps {
                echo '📦 Lấy files publish từ Docker image...'
                sh """
                    rm -rf ${PUBLISH_DIR}
                    docker create --name temp_extract ${DOCKER_HUB_USER}/${IMAGE_NAME}:latest
                    docker cp temp_extract:/app ${PUBLISH_DIR}
                    docker rm temp_extract
                    echo "✅ Files publish:"
                    ls -la ${PUBLISH_DIR}
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
                    sh '''
                        # Tạo app_offline.htm để IIS dừng app trước khi deploy
                        echo '<html><body><h1>Deploying, please wait...</h1></body></html>' \
                            > /tmp/app_offline.htm

                        lftp -u "$FTP_USER","$FTP_PASS" ftp://site8642.siteasp.net <<LFTP
set ssl:verify-certificate no
set ftp:ssl-allow yes
set ftp:passive-mode on
set ftp:prefer-epsv no
set net:max-retries 5
set net:timeout 60
set xfer:clobber yes
set mirror:parallel-transfer-count 2

# Upload app_offline.htm trước để dừng app
put /tmp/app_offline.htm -o /wwwroot/app_offline.htm

# Upload toàn bộ files
mirror -R --delete --continue --no-perms \
    --exclude app_offline.htm \
    /var/jenkins_home/workspace/notaion-backend/publish_output \
    /wwwroot

# Xóa app_offline.htm để app chạy lại
rm -f /wwwroot/app_offline.htm

bye
LFTP

                        echo "✅ FTP deploy hoàn tất!"
                    '''
                }
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

        stage('Deploy Container (local)') {
            steps {
                echo '🚀 Đang chạy container local...'
                sh """
                    docker stop ${CONTAINER_NAME} || true
                    docker rm   ${CONTAINER_NAME} || true
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
            echo "🌐 Local:      http://localhost:${APP_PORT}/api/DeployInfo/info"
            echo "🌐 MonsterASP: http://notaion.runasp.net/api/DeployInfo/info"
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
