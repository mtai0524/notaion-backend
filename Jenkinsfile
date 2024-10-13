pipeline
{
    agent any
    stages 
    {
        stage('Cloning') 
        {
            steps {
                git branch : 'main', credentialsId: '3ceb09c4-d257-4d6b-b65c-26db994addff', url :'https://github.com/mtai0524/notaion-backend.git'
            }
        }
        
        stage('Restore package')
        {
            steps
            {
                bat 'dotnet restore .\\NotaionWebApp\\Notaion\\Notaion.csproj'
            }
        }
        
        stage('Build')
        {
            steps
            {
                bat 'dotnet build .\\NotaionWebApp\\Notaion\\Notaion.csproj --configuration Release'
            }
        }
        
        stage('Publish')
        {
            steps
            {
                bat 'dotnet publish .\\NotaionWebApp\\Notaion\\Notaion.csproj'
            }
        }
        
        stage('Stop service')
        {
            steps{
                bat '%windir%\\system32\\inetsrv\\appcmd stop sites notaion2.com.vn'
                bat '%windir%\\system32\\inetsrv\\appcmd stop apppool /apppool.name:notaion2.com.vn'
                bat 'echo waiting until service stopped'
                bat 'ping google.com /n 5'
            }
        }
        
        stage('Copy iis')
        {
            steps{
                bat 'xcopy .\\NotaionWebApp\\Notaion\\bin\\Debug\\net7.0\\publish T:\\IIS_NOTAION /e /y /i /r'
            }
        }
        
        stage('Start service')
        {
            steps{
                bat '%windir%\\system32\\inetsrv\\appcmd start sites notaion2.com.vn'
                bat '%windir%\\system32\\inetsrv\\appcmd start apppool /apppool.name:notaion2.com.vn'
            }
        }
       
    }
}
