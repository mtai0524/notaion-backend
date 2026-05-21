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

        stage('Deploy to MonsterASP') {
            steps {
                echo '🚀 Đang deploy lên MonsterASP...'
                withCredentials([usernamePassword(
                    credentialsId: 'monsterasp-creds',
                    usernameVariable: 'DEPLOY_USER',
                    passwordVariable: 'DEPLOY_PASS'
                )]) {
                    sh '''
                        # Lấy file publish từ image đã build sẵn
                        docker create --name temp_extract mtaidev/notaion-backend:latest
                        docker cp temp_extract:/app ./publish_output
                        docker rm temp_extract
        
                        # Nén lại
                        cd publish_output && zip -r ../deploy.zip . && cd ..
        
                        # Deploy lên MonsterASP qua WebDeploy
                        curl -k -X POST \
                            "https://site8642.siteasp.net:8172/msdeploy.axd?site=site8642" \
                            -u "$DEPLOY_USER:$DEPLOY_PASS" \
                            --data-binary @deploy.zip \
                            -H "Content-Type: application/zip"
        
                        # Dọn dẹp
                        rm -rf publish_output deploy.zip
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
                        echo $DOCKER_PASS | docker login -u $DOCKER_USER --password-stdin
                        docker push ${DOCKER_HUB_USER}/${IMAGE_NAME}:${IMAGE_TAG}
                        docker push ${DOCKER_HUB_USER}/${IMAGE_NAME}:latest
                    """
                }
            }
        }

       stage('Deploy') {
           steps {
               echo '🚀 Đang deploy container...'
               sh """
                   docker stop ${CONTAINER_NAME} || true
                   docker rm   ${CONTAINER_NAME} || true
                   docker pull ${DOCKER_HUB_USER}/${IMAGE_NAME}:latest
                   docker run -d \
                       --name ${CONTAINER_NAME} \
                       --restart always \
                       -p 8081:8080 \
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
            echo "🌐 App đang chạy tại: http://localhost:${APP_PORT}"
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