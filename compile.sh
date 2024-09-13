g++ -I./GL/ -I./sml/include/ -I./include/ -I. -I./assimp/include/ ./src/*.c *.cpp -L./assimp/lib -lGL -lGLEW -lglfw -lX11 -lassimp -lstdc++fs -o main
##export LD_LIBRARY_PATH=:$LD_LIBRARY_PATH:./assimp/lib/:./assimp/lib/
##g++ -I./GL/ -I./sml/include/ -I./include/ -I. ./src/*.c *.cpp -lGL -lGLEW -lglfw -lX11 -lassimp -lstdc++fs -o main
