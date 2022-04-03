pipeline {
    	
	agent any
	 
	environment {
		PROJECT      = './Booth.DocketTest/Booth.DocketTest.csproj'
    }

    stages {
		stage('Build') {
			agent { 
				docker { 
					image 'mcr.microsoft.com/dotnet/sdk:6.0-alpine' 
					reuseNode true
				}
			}

			stages {
				stage('Build') {
					steps {
						sh "dotnet build ${PROJECT} --configuration Release"
					}
				}


				stage('Publish') {
					steps {
						sh "dotnet publish ${PROJECT} --configuration Release --output ./deploy"
					}
				}
			}

		}
		
		stage('Deploy') {
			steps {
				script {
					def dockerImage = docker.build("craigbooth/dockertest")
				}
            }
		}
    }
	
	post {
		success {
			cleanWs()
		}
	}
}