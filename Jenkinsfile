pipeline {
    	
	agent any
	 
	environment {
		PROJECT      = './Booth.DockerTest/Booth.DockerTest.csproj'
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
					httpRequest httpMode: 'POST', responseHandle: 'NONE', url: 'https://portainer.boothfamily.id.au/api/webhooks/2edd15ff-a183-4055-9355-54656aaa7066', wrapAsMultipart: false
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