## Lua的性能分析器 For Unity
<br/>

### 设计目的
unity配合上lua脚本可以说是目前最流行的热更新框架了，但是因为缺少对应的性能分析器，使很多团队在优化性能的时候简直不知道从何查起，而一些项目往往会通过削减lua脚本的使用来达到性能优化的目的，这样做项目是得到了优化，不过也带来了大量不可热更的代码，降低了运营灵活度。<br>
而这个工具就是要给在Unity、甚至其他游戏引擎提供一套很好用的性能检测工具，帮助使用lua脚本开发的游戏早日上线以及稳定运营。

### 部署安装
目前支持XLua、SLua、ToLua。这个版本是远程调试版，支持Windows、Android、IL2CPP、IOS的真机Profiler。

- 打开LuaProfiler目录
- 把`LuaProfilerClient`文件夹Copy到项目工程目录下，如果你把Lua的代码放到了Plugins下，那么也一起挪过去，必须要保证代码的DLL跟Lua库打在一个DLL中。
- 用Unity5.6以上的版本新建一个工程把`LuaProfilerServer`Copy到工程中。

### 原理
本工具使用mono.ceil的IL注入功能(XLUA的热修复原理)，在代码编译完成后对代码进行hook改造，在C#还有Lua的代码中强行在对应的开始结束位置插入Profiler代码，然后统计得到统计结果。

## 


### 使用教程

#### 开启

游戏工程中打开 "Window->Lua Profiler Window"配置窗口，勾选要profiler的内容，以及配置本机的IP地址。
## 
![](doc/config_client.png)

Profiler编辑器工程中也打开profiler窗口，然后OpenService，等待客户端连接
## 
![](doc/config_server.png)

## 
![](doc/profiler.gif)
## 

#### Record模式
&nbsp;&nbsp;&nbsp;&nbsp;这个版本的Record模式比较轻量，直接点击Record,然后游戏工程和Profiler编辑器对接上后，点击StartRecord即可开始记录Record,再次点击即可关闭Record模式。

##### 按钮功能介绍


- 拉动滑条可以快速大概的调整sample帧
- 点击 __'<'__ 、 __'>'__ 两个按钮一次只增加或减少1帧
- 点击 __'<<'__ 、 __'>>'__ 两个按钮可以快速移动到效率出了问题的某些帧
- 设置Capture Lua GC、Capture Mono GC、Frame Count可以设置问题阈值
- Save跟Load 两个按钮可以保存和载入Sample的采样信息

使用效果
![](doc/profiler.gif)
<br/>

#### 打包真机
设置打包宏`USE_LUA_PROFILER`即可开关打包宏，lua代码如果用使用luac或者luajit请使用Tools下的InjectLua.exe对代码进行hook插桩

```
InjectLua.exe "inpath" "outpath"
```

#### 使用项目
![](doc/ljjc.jpg)

## 
有什么BUG可以联系加群：[882425563](https://jq.qq.com/?_wv=1027&k=5QkOBSc)

## 
### 感谢

#### 关键问题解决者
[Xavier](https://github.com/starwing)

#### 关键测试的成员
[Jay](https://github.com/Jayatubi) <br/>
[ZhangDi](https://github.com/ZhangDi2018) <br/>
以及[LuaProfiler](https://jq.qq.com/?_wv=1027&k=5QkOBSc)群中的所有群成员
