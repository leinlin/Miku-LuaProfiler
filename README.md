## Lua Profiler For Unity

![](doc~/logo.png)

### 支持情况
| 系统                  | 支持情况|
| ----------------------- | ------------ |
| `windows`| ✔  |
| `android`| ✔  |
| `MAC`| ✔  |
| `IOS`| 支持中  |

![](doc~/use.gif)

### 部署和安装
1. 推荐安装Unity2019可以直接用PackageManager 用add by git路径添加,国内网络不好可以使用gitee代理
![](doc~/package_clone.png)
>https://github.com/leinlin/Miku-LuaProfiler.git

2. Unity5.6以上版本建议把代码clone下来，然后把LuaProfiler目录copy到Assets中

3. 看到这个菜单说明安装成功了
![](doc~/install_success.png)

### 如何使用
- 编辑器使用推荐使用local模式，打开Profiler窗口运行游戏即可，去掉了deeplua这个开关
![](doc~/2022-12-14-21-50-20.png)

- 真机目前只支持android系统，打包的时候加上宏 `USE_LUA_PROFILER`
- 打包后，在Unity的Application.persistentDataPath 目录下面建立 文件 need_hook_miku_lua，使用后会自动删除这个文件，第二次如果需要luaprofiler请重新创建
 

可以参考下面的指令
```
adb shell
cd /sdcard/Android/data/{package_name}/files
touch need_hook_miku_lua
```

编辑器中点击local mode 按钮就可以切换remote mode,输入手机ip就可以查看数据了

![](doc~/2022-12-14-22-04-21.png)
如果公司网络不允许你直接访问手机ip，那么可以用USB数据线连接手机和电脑输入以下指令
```
adb forward tcp:2333 tcp:2333
```
之后直接在ip栏中输入`127.0.0.1`即可

- 这个工程hook了dlopen去寻找lua的native库，因为dlopen是系统库可能会导致莫名其妙的闪退，为了避免这个问题，我们可以自己指定lua库
- 方法在CSharp-Assembly.dll 所在的库中建立如下脚本（注意防裁剪）
```
#if USE_LUA_PROFILER && UNITY_ANDROID && !UNITY_EDITOR
namespace MikuLuaProfiler
{
    public class CustomLua : ILuaCustomSetting
    {
        const string LIB_NAME = "miku_hook";
        
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr atri_dlopen(string path, int mode);

        public IntPtr GetPtr()
        {
            // 其他不用改就把 libxlua.so改了就好了
            IntPtr result = atri_dlopen("libxlua.so", 2);
            return result;
        }
    }
}
#endif
```

### 数据说明
| Name                    | Descriptions                                                                                              |
| ----------------------- | -------------------------------------------------------------------------------------------------------   |
| `Overview`              | 函数名称                                                                                                  |
| `totalLuaMemory`        | 此函数生成的所有Lua GC的总和                                                                              |
| `self`                  | 函数本身产生的GC量                                                                                        |
| `totalMonoMemory`       | 此函数生成的所有Mono GC的总和                                                                             |
| `self`                  | 函数本身产生的GC量                                                                                        |
| `currentTime`           | 函数在当前帧中运行所需的时间                                                                              |
| `averageTime`           | 计算在函数上花费的时间的平均值                                                                            |
| `totalTime`             | 此功能消耗的所有时间                                                                                      |
| `LuaGC`                 | 由当前帧生成的Lua GC                                                                                      |
| `MonoGC`                | 由当前帧生成的Mono GC                                                                                     |
| `totalCalls`            | 游戏开始后此功能的运行次数                                                                                |
| `Calls`                 | 函数当前帧的执行次数                                                                                      |

### FAQ

有的Unity版本不支持 `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]`这个属性的时候，你可以在lua虚拟机启动之前调用代码
```
MikuLuaProfiler.HookLuaSetup.OnStartGame();
```

打开DeepLua之后会有很多空异常报错
> 有的项目组会使用debug.getupvalue去获取变量并进行修改，但是MikuLuaProfiler会在所有代码执行之前添加一个Upvalue函数进行BeginSample和EndSample。这个会导致原先的upvalue表混乱，解决办法就是遍历Upvalue表，然后根据upvalue的命名获取到相应的变量，如下代码：
```
function debug.getupvalue_byname(func, name)
    local i = 1
    while true do
        local n, v = debug.getupvalue(func, i)
        if not n then break end
        if n == name then return v end
        i = i + 1
    end
end

function debug.setupvalue_byname(func, name, value)
    local i = 1
    while true do
        local n, v = debug.getupvalue(func, i)
        if not n then break end
        if n == name then
            debug.setupvalue(func, i, value)
            break
        end
        i = i + 1
    end
end
```

真机数据只有resume或者协程数据
> 不要用luac加密代码，请直接使用明码字符串


运行了性能调试之后，用代码调用luaGC无效
> 为了防止lua的GC导致内存统计出现负数，本项目直接用hook技术接管了lua的GC函数，让用户的自动促发无效，GC会根据内存是否增长到上次GC的1.2倍左右就触发，如果项目的Update GC优化的较好，内存曲线是基本一致的。


关于XLua demo跑不起来的问题
> 把Demo里面LuaBehaviour 中 internal static LuaEnv luaEnv = new LuaEnv()的赋值改到Awake里面

关于自定义profiler点
```
local LuaProfiler = MikuLuaProfiler.LuaProfiler
LuaProfiler.BeginSampleCustom("profiler name")
-- your code
LuaProfiler.EndSampleCustom()
```


MarkStaticRecord、MarkLuaRecord、DiffRecord、ClearDiff有什么用
>这一组功能是查泄漏使用的。举一个例子，你可以在打开UI前MarkStaticRecord，进行一次内存快照，然后打开UI后MarkLuaRecord，记录第二次快照，最后你在调用UI释放的函数这里DiffRecord。如果打开UI前不持有，而打开UI后
与释放UI后都持有的对象为泄漏。
函数API为：
```
function miku_do_record(val, prefix, key, record, history, null_list, staticRecord)

function miku_diff(record, staticRecord)
```

运行起来后totalLuaMemory为负数
>底层统计lua内存申请采用记录lua虚拟机总量来记录对应的GC，如果函数运行的中间发生了GC就会导致内存差值为负数，你可以把自动GC关闭掉后进行统计。

如何对Lua函数进行各种数据排序
>在搜索框中输入：[lua],然后点击右上角的merge按钮,最后点击各个数据标签进行排序
![](doc~/sort.png)

如何定位Lua函数内存暴涨的点
>使用Record模式启动游戏，在想采样的地方点击StartRecord,截取完记录后，找一段明显内存上涨的地方，采样出来分析函数即可.（鼠标点击上去，然后按键盘的左右键）
![](doc~/record.png)

ref的function代表什么
>这里存储的一般都是C#这里持有的回调函数，你可以每一次打开UI前先clear数据，然后进入UI后记录数据，释放UI后如果 还持有不少委托，就代表说泄漏了。

<span id="contact_zh"></span>
### [联系](#zh)
如果您发现任何错误或有任何建议加入QQ群：[882425563](https://jq.qq.com/?_wv=1027&k=5QkOBSc) 与我们联系

<span id="contact_zh"></span>
## 投食
![](doc~/zfb.png)

##
![](doc~/meizi.gif)

