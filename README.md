# 项目使用说明

## 目录结构

- assets: 存放图片文件
- src：后端代码
- webclient：前端代码
- inception5h：模型

## 所需环境

- Docker
- Tensorflow 1.x
- Dotnet Core 3.x
- Node 10+

## 如何启动

1. 启动 `milvus` 和 `redis`：
   
    ```bash
    docker-compose up -d
    ```
2. 启动前端
   
   ```bash
    cd ./webclient
    npm i
    npm start
    cd ..
   ```
3. 启动后端

    ```bash
    dotnet run
    ```