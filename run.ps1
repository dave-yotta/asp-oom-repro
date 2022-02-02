docker build . -t app
docker run --rm -p 5000:80 -m 64m -it app
docker rmi app
