# プログラム概要
遠隔地のカメラ映像をヘッドマウントディスプレイで見ることができるアプリケーションの受信側。受信した360度映像と深度データを表示し、HMD(Oculus Rift S)で見ることができる。深度データは3次元点群に変換され360度映像に立体感を与える。言語はC#、Cg言語を使用し、Unityを用いて開発した。
# 主要シーン・スクリプト説明
- Viewer：
    基本的な機能を実装したシーン。360度映像と点群をHDMにより高い臨場感で見ることができる。
    - ClientTest.cs
        - 深度データの受信・処理を行うスクリプト。深度データから生成した点群のメッシュへの適用、各点の色の変更などを行う。
    - ThetaC.cs
        - 360度映像の受信・処理を行うスクリプト。受信した360度映像はテクスチャに変換され、球体の内側に貼り付けられる。球体にはICO球を用いた。
    - TcpC.cs
        - TCP通信の接続と受信処理を行うクラス。クライアント側。
    - Sprite-Geometry.shader
        - 点群の見やすさを向上するために制作したシェーダー。ジオメトリシェーダーで元の点群を上下左右に微小にずらして同時に表示することで、点の密度を高め見やすさを高めた。

その他のシーン・スクリプトは実験用もしくはマイナーチェンジ版。
# 使用ライブラリ等
- Intel RealSense SDK2.0 (https://www.intelrealsense.com/sdk-2/)
    - 深度データの処理に使用。
- Oculus Integration (https://assetstore.unity.com/packages/tools/integration/oculus-integration-82022)
    - HDMの導入に使用。
- octahedron-sphere-meshes.unitypackage (https://catlikecoding.com/unity/tutorials/octahedron-sphere/)
    - ICO球として使用。