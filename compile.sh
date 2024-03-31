g++ -I./GL/ -I./sml/include/ -I./include/ -I. -I./assimp/include/ ./src/*.c *.cpp -L./assimp/lib -lGL -lGLEW -lglfw3 -lX11 -lassimp -lstdc++fs -o main
##export LD_LIBRARY_PATH=:$LD_LIBRARY_PATH:./assimp/lib/:./assimp/lib/
