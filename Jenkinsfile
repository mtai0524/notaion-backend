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
                    sh '''
                        echo "$DOCKER_PASS" | docker login -u "$DOCKER_USER" --password-stdin
                        docker push ''' + "${DOCKER_HUB_USER}/${IMAGE_NAME}:${IMAGE_TAG}" + '''
                        docker push ''' + "${DOCKER_HUB_USER}/${IMAGE_NAME}:latest" + '''
                    '''
                }
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
                        set -e

                        # Lấy file publish từ image vừa build
                        rm -rf publish_output
                        docker create --name temp_extract mtaidev/notaion-backend:latest
                        docker cp temp_extract:/app ./publish_output
                        docker rm temp_extract

                        # app_offline.htm: bảo IIS dừng app trước khi ghi đè (tránh lock .dll)
                        cat > publish_output/app_offline.htm <<'OFFLINE'
<!doctype html>
<html><body><h1>Deploying new version...</h1></body></html>
OFFLINE

                        # Tar workspace và stream vào Alpine container có lftp.
                        # Dùng stdin pipe để không phải mount volume (tránh lỗi path khi Jenkins chạy trong Docker - DooD).
                        tar -C publish_output -cf - . | docker run --rm -i \
                            -e FTP_USER \
                            -e FTP_PASS \
                            -e FTP_HOST \
                            -e FTP_REMOTE_DIR \
                            alpine:3.20 sh -c '
                                apk add --no-cache lftp tar >/dev/null
                                mkdir -p /data && tar -xf - -C /data
                                lftp -u "$FTP_USER","$FTP_PASS" "ftp://$FTP_HOST" <<LFTP
set ssl:verify-certificate no
set ftp:ssl-allow yes
set ftp:ssl-protect-data yes
set net:max-retries 3
set net:timeout 20
put -O "$FTP_REMOTE_DIR" /data/app_offline.htm
mirror -R --delete --parallel=4 --verbose --exclude-glob app_offline.htm /data "$FTP_REMOTE_DIR"
rm -f "$FTP_REMOTE_DIR/app_offline.htm"
bye
LFTP
                            '

                        rm -rf publish_output
                        echo "✅ Đã deploy lên MonsterASP."
                    '''
                }
            }
        }

        stage('Deploy Container (local)') {
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
