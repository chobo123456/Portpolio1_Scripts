using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.IO;
using System.Diagnostics;
public class EventBus
{
    private static Dictionary<string, Delegate> events = new();

    public static bool HasEvent(string eventName)
    {
        return events.ContainsKey(eventName);
    }
    public static void Sub(string eventName, Action action)
    {
        if(events.TryGetValue(eventName, out var recentEvents))
        {
            var mergeEvent = Delegate.Combine(recentEvents, action);
            events[eventName] = mergeEvent;
        }
        else
            events[eventName] = action;
    }

    public static void UnSub(string eventName, Action action)
    {
        if(events.TryGetValue(eventName, out var recentEvents))
        {
            var removeEvent = Delegate.Remove(recentEvents, action);
            events[eventName] = removeEvent;

            if(events[eventName] == null) events.Remove(eventName);
        }
    }

    public static void Invoke(string eventName)
    {
        if(string.IsNullOrEmpty(eventName)) return;

        if(events.TryGetValue(eventName, out var recentEvents))
        {
            if(recentEvents is Action action)
            {
                action.Invoke();
            }
            else if(recentEvents != null)
            {
                recentEvents.DynamicInvoke();
            }
            else
            {
                return;
            }
        }
    }

    public static void Sub<T>(string eventName, Action<T> action)
    {
        if(events.TryGetValue(eventName, out var recentEvents))
        {
            var mergeEvent = Delegate.Combine(recentEvents, action);
            events[eventName] = mergeEvent;
        }
        else
            events[eventName] = action;
    }

    public static void UnSub<T>(string eventName, Action<T> action)
    {
        if(events.TryGetValue(eventName, out var recentEvents))
        {
            var removeEvent = Delegate.Remove(recentEvents, action);
            events[eventName] = removeEvent;

            if(events[eventName] == null) events.Remove(eventName);
        }
    }

    public static void Invoke<T>(string eventName, T args)
    {
        if(string.IsNullOrEmpty(eventName)) return;

        if(events.TryGetValue(eventName, out var recentEvents))
        {
            if(recentEvents is Action<T> action)
            {
                action.Invoke(args);
            }
            else if(recentEvents != null)
            {
                recentEvents.DynamicInvoke(args);
            }
        }
    }

    //T, T1
    public static void Sub<T, T1>(string eventName, Action<T, T1> action)
    {
        if(events.TryGetValue(eventName, out var recentEvents))
        {
            var mergeEvent = Delegate.Combine(recentEvents, action);
            events[eventName] = mergeEvent;
        }
        else
            events[eventName] = action;
    }

    public static void UnSub<T, T1>(string eventName, Action<T, T1> action)
    {
        if(events.TryGetValue(eventName, out var recentEvents))
        {
            var removeEvent = Delegate.Remove(recentEvents, action);
            events[eventName] = removeEvent;

            if(events[eventName] == null) events.Remove(eventName);
        }
    }

    public static void Invoke<T, T1>(string eventName, T args1, T1 args2)
    {
        if(string.IsNullOrEmpty(eventName)) return;

        if(events.TryGetValue(eventName, out var recentEvents))
        {
            if(recentEvents is Action<T, T1> action)
            {
                action.Invoke(args1, args2);
            }
            else if(recentEvents != null)
            {
                recentEvents.DynamicInvoke(args1, args2);
            }
        }
    }

    //T, T1, T2
    public static void Sub<T, T1, T2>(string eventName, Action<T, T1, T2> action)
    {
        if(events.TryGetValue(eventName, out var recentEvents))
        {
            var mergeEvent = Delegate.Combine(recentEvents, action);
            events[eventName] = mergeEvent;
        }
        else
            events[eventName] = action;
    }

    public static void UnSub<T, T1, T2>(string eventName, Action<T, T1, T2> action)
    {
        if(events.TryGetValue(eventName, out var recentEvents))
        {
            var removeEvent = Delegate.Remove(recentEvents, action);
            events[eventName] = removeEvent;

            if(events[eventName] == null) events.Remove(eventName);
        }
    }

    public static void Invoke<T, T1, T2>(string eventName, T args1, T1 args2, T2 args3)
    {
        if(string.IsNullOrEmpty(eventName)) return;

        if(events.TryGetValue(eventName, out var recentEvents))
        {
            if(recentEvents is Action<T, T1, T2> action)
            {
                action.Invoke(args1, args2, args3);
            }
            else if(recentEvents != null)
            {
                recentEvents.DynamicInvoke(args1, args2, args3);
            }
        }
    }

    //--Funcs--

    public static void Sub_Func<T>(string eventName, Func<T> action)
    {
        if(events.TryGetValue(eventName, out var recentEvents))
        {
            var mergeEvent = Delegate.Combine(recentEvents, action);
            events[eventName] = mergeEvent;
        }
        else
            events[eventName] = action;
    }

    public static void UnSub_Func<T>(string eventName, Func<T> action)
    {
        if(events.TryGetValue(eventName, out var recentEvents))
        {
            var removeEvent = Delegate.Remove(recentEvents, action);
            events[eventName] = removeEvent;

            if(events[eventName] == null) events.Remove(eventName);
        }
    }

    public static T Invoke_Func<T>(string eventName)
    {
        if(string.IsNullOrEmpty(eventName))
        {
            Util.Log("Invoke_Func 오류 발생, eventName이 널이거나 없음");
            return (T)default;
        } 

        if(events.TryGetValue(eventName, out var recentEvents))
        {
            if(recentEvents is Func<T> func)
            {
                return func.Invoke();
            }
        }

        return (T)default;
    }

    public static void Sub_Func<T, T1>(string eventName, Func<T, T1> action)
    {
        if(events.TryGetValue(eventName, out var recentEvents))
        {
            var mergeEvent = Delegate.Combine(recentEvents, action);
            events[eventName] = mergeEvent;
        }
        else
            events[eventName] = action;
    }

    public static void UnSub_Func<T, T1>(string eventName, Func<T, T1> action)
    {
        if(events.TryGetValue(eventName, out var recentEvents))
        {
            var removeEvent = Delegate.Remove(recentEvents, action);
            events[eventName] = removeEvent;

            if(events[eventName] == null) events.Remove(eventName);
        }
    }

    public static T1 Invoke_Func<T, T1>(string eventName, T args)
    {
        if(string.IsNullOrEmpty(eventName))
        {
            Util.Log("Invoke_Func 오류 발생, eventName이 널이거나 없음");
            return (T1)default;
        } 

        if(events.TryGetValue(eventName, out var recentEvents))
        {
            if(recentEvents is Func<T, T1> func)
            {
                return func.Invoke(args);
            }
        }

        return (T1)default;
    }


    public static void Sub_Func<T, T1, T2>(string eventName, Func<T, T1, T2> action)
    {
        if(events.TryGetValue(eventName, out var recentEvents))
        {
            var mergeEvent = Delegate.Combine(recentEvents, action);
            events[eventName] = mergeEvent;
        }
        else
            events[eventName] = action;
    }
    public static void UnSub_Func<T, T1, T2>(string eventName, Func<T, T1, T2> action)
    {
        if(events.TryGetValue(eventName, out var recentEvents))
        {
            var removeEvent = Delegate.Remove(recentEvents, action);
            events[eventName] = removeEvent;

            if(events[eventName] == null) events.Remove(eventName);
        }
    }

    public static T2 Invoke_Func<T, T1, T2>(string eventName, T args1, T1 args2)
    {
        if(string.IsNullOrEmpty(eventName))
        {
            Util.Log("Invoke_Func 오류 발생, eventName이 널이거나 없음");
            return (T2)default;
        } 
        
        if(events.TryGetValue(eventName, out var recentEvents))
        {
            if(recentEvents is Func<T, T1, T2> func)
            {
                return (T2)func.Invoke(args1, args2);
            }
        }

        return (T2)default;
    }
}

public class SingleShotEvent
{
    private List<System.Action> methods;

    public static SingleShotEvent operator +(SingleShotEvent evt, System.Action action)
    {
        if(evt == null) evt = new();
        if(evt.methods == null) evt.methods = new();

        evt.methods.Add(action);
        return evt;
    }

    public void Invoke()
    {
        for(int i = 0; i < methods.Count; i++)
            methods[i].Invoke();

        methods.Clear();
    }
}

public class AddressableUtil
{
    private static List<AsyncOperationHandle> handles = new();
    private static List<AsyncOperationHandle> instantHandles = new();
    private static HashSet<string> loadHash = new();
    public static async Task<T> Load<T>(string address)
    {
        var handle = Addressables.LoadAssetAsync<T>(address);

        await handle.Task;

        if(handle.Status == AsyncOperationStatus.Succeeded)
        {
            handles.Add(handle);
            return handle.Result;
        }
        else
        {
            if(handle.IsValid()) Addressables.Release(handle);
            return (T)default;
        }
    }

    public static async Task<T> Load_Instant<T>(string address, CancellationToken token = default)
    {
        if(loadHash.Contains(address)) return (T)default;

        var handle = Addressables.LoadAssetAsync<T>(address);

        try
        {
            await handle.Task;

            if(handle.Status == AsyncOperationStatus.Succeeded)
            {
                if(handle.IsValid())
                {
                    instantHandles.Add(handle);
                    loadHash.Add(address);

                    token.Register(()=>
                    {
                        Addressables.Release(handle);

                        instantHandles.Remove(handle);
                        if(instantHandles.Count <= 0) instantHandles.Clear();

                        loadHash.Remove(address);
                        if(loadHash.Count <= 0) loadHash.Clear();
                    });
                }
                
                return handle.Result;
            }
            else
            {
                if(handle.IsValid()) Addressables.Release(handle);
                return (T)default;
            }
        }
        catch(System.OperationCanceledException)
        {
            if(handle.IsValid()) Addressables.Release(handle);
            return (T)default;
        }
    }

    public static void ClearHandle()
    {
        foreach(var handle in handles)
        {
            if(handle.IsValid())
                Addressables.Release(handle);
        }

        handles.Clear();
    }
}

public static class CancellationUtil
{
    public static CancellationToken GetCancelOnDestroy(this MonoBehaviour target)
    {
        return target.destroyCancellationToken;
    }
}

public class JsonUtil
{
    public static T ParseFromJson<T>(TextAsset asset)
    {
        return JsonUtility.FromJson<T>(asset.text);
    }

    public static T ParseFromJson<T>(string str)
    {
        return JsonUtility.FromJson<T>(str);        
    }

    public static string ParseToJson(object targetArgs)
    {
        return JsonUtility.ToJson(targetArgs, true);
    }

    public static string Combine_Path(string str1, string str2)
    {
        string path = Path.Combine(str1, str2);

        return path;
    }

    public static bool IsExistFile(string filePath)
    {
        return File.Exists(filePath);
    }
    public static bool IsExistDirectory(string directoryPath)
    {
        return Directory.Exists(directoryPath);
    }

    public static void MakeDirectory(string directoryPath)
    {
        Directory.CreateDirectory(directoryPath);
    }

    public static void FileWrite(string path, string writeText)
    {
        File.WriteAllText(path, writeText);
    }
    public static string FileRead(string path)
    {
        return File.ReadAllText(path);
    }
    public static void FileMove(string path1, string path2)
    {
        File.Move(path1, path2);
    }

    public static void FileCopy(string path1, string path2, bool isOverwrite = true)
    {
        File.Copy(path1, path2, overwrite : isOverwrite);
    }
}
public class RandomNumber
{
    private static int _counter = 0;
    private static Save<int> save;
    
    public static List<int> savedNumber = new();
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        save = new("System", "InstanceId");
        
        if(save.IsExist())
            _counter = save.savedData;
    }
    public static int GetNumber(List<int> argsNumber = null)
    {
        if(argsNumber != null) {
            for(int i = 0; i < argsNumber.Count; i++)
            {
                int curNum = argsNumber[i];
                if(!savedNumber.Contains(curNum))
                    savedNumber.Add(curNum);
            }
        }

        System.DateTime moment = System.DateTime.UtcNow;
        int year = moment.Year;
        int month = moment.Month;
        int day  = moment.Day;
        int hour = moment.Hour;
        int minute = moment.Minute;
        int second = moment.Second;
        int milliSecond = moment.Millisecond;
        
        int randomNumber = (year + month + day + hour + minute + second + milliSecond) + _counter;

        _counter++;
        save.Saving(_counter);

        return randomNumber;
    }
}

public class PlayerPref
{
    public static void SetPlayerPref<T>(string str, object obj)
    {
        if(typeof(T) == typeof(int))
        {
            PlayerPrefs.SetInt(str, (int)obj);
        }
        else if(typeof(T) == typeof(float))
        {
            PlayerPrefs.SetFloat(str, (float)obj);
        }
        else if(typeof(T) == typeof(string))
        {
            PlayerPrefs.SetString(str, (string)obj);
        }
    }

    public static T GetPlayerPref<T>(string str, object baseValue = default)
    {
        if(typeof(T) == typeof(int))
        {
            int castDefaultValue = baseValue != null ? (int)baseValue : 0;
            return (T)(object)PlayerPrefs.GetInt(str, castDefaultValue);
        }
        else if(typeof(T) == typeof(float))
        {
            float castDefaultValue = baseValue != null ? (float)baseValue : 0f;
            return (T)(object)PlayerPrefs.GetFloat(str, castDefaultValue);
        }
        else if(typeof(T) == typeof(string))
        {
            string castDefaultValue = baseValue != null ? (string)baseValue : "";
            return (T)(object)PlayerPrefs.GetString(str, castDefaultValue);
        }

        return (T)default;
    }

    public static void PlayerPrefSave()
    {
        PlayerPrefs.Save();
    }
    public static bool HasPlayerPref(string str)
    {
        return PlayerPrefs.HasKey(str);
    }
}


public class ReactiveProperty
{
    private object cur_value;

    public object Value {
        get
        {
            return cur_value;
        }
        set
        {
            this.cur_value = value;

            for(int i = 0; i < methods.Count; i ++)
            {
                var method = methods[i];

                if(method != null)
                {
                    method.Invoke();
                }  
            }
        }
    }
    private List<Action> methods = new();

    public void Subscribe(Action methodArgs)
    {
        methods.Add(methodArgs);
    }   
}
public class ReactiveProperty<T>
{
    private ReadOnlyReactiveProperty<T> readonlyProperty;
    public ReactiveProperty(T argument)
    {
        cur_value = argument;
        readonlyProperty = new(this);
    }
    private T cur_value;

    public T Value {
        get
        {
            return cur_value;
        }
        set
        {
            this.cur_value = value;

            if(methods != null)
            {
                T args = cur_value;
                var invokeMethod = (methods as Action<T>);

                invokeMethod.Invoke(args);
            }
        }
    }
    private Delegate methods;

    public void Subscribe(Action<T> methodArgs)
    {
        methods = Delegate.Combine(methods, methodArgs);
    }   

    public void UnSubscribe(Action<T> methodArgs)
    {
        methods = Delegate.Remove(methods, methodArgs);
    }

    public ReadOnlyReactiveProperty<T> ToReadOnlyValue()
    {
        return readonlyProperty;
    }
}

public class ReadOnlyReactiveProperty<T>
{
    private readonly ReactiveProperty<T> property;
    public ReadOnlyReactiveProperty(ReactiveProperty<T> property)
    {
        this.property = property;
    }

    public T Value {
        get
        {
            return (T)property.Value;
        }
    }

    public void Subscribe(Action<T> methodArgs)
    {
        property.Subscribe(methodArgs);
    }   

    public void UnSubscribe(Action<T> methodArgs)
    {
        property.UnSubscribe(methodArgs);
    } 
}

public static class FindFromChildren
{
    public static Transform FindTarget(this Transform targetTr, string findChildName)
    {
        Transform result = targetTr.Find(findChildName);

        if(result != null)
            return result;

        for(int i = 0; i < targetTr.childCount; i++)
        {
            Transform child = targetTr.GetChild(i);

            if(child.childCount <= 0) continue;

            Transform child_result = child.FindTarget(findChildName);

            if(child_result != null)
            {
                result = child_result;
                break;
            }
        }

        return result;
    }
}

public static class Util
{
    [Conditional("LogAble")]
    public static void Log(string context, string color = "white")
    {
        UnityEngine.Debug.Log($"<color={color}>{context}</color>");
    }
}
public static class ExceptionUtil
{
    [Conditional("LogAble")]
    public static void Exception(this object cls, string context)
    {
        throw new Exception(context);
    }
}

public static class CoroutineUtil
{
    //StartCoroutine같이 그냥사용할수도있지만 매번 클래스에 Coroutine을 적어야하는 경우가 있음
    //또한 매번 코루틴이 중복 실행되면 안되는경우에 앞줄에 StopCoroutine을 적어야함, 이점에서 가독성 저하 가능성이있다고 보였고
    //그럼으로 이클래스를 작성해서 중복 실행방지하는코드를 작성함
    //그럼 그냥 mono.StartCoroutine만 있는 클래스는 무엇이냐 하냐면 
    //StartCoroutine이라고 적어도 아무 문제는 없으나 문장을 통일하려고 적었음,
    //아무래도 어느건 this.RunRoutine이고 어느건 StartCoroutine이면 문장이 통일 안되서 바로 알기 어렵지않을까 싶어서 통일시키려고 적음
    private static Dictionary<(MonoBehaviour mono, string coroutineName), Coroutine> routines = new();
    public static Coroutine RunRoutine(this MonoBehaviour mono, System.Collections.IEnumerator coroutine, Coroutine routine)
    {
        if(routine != null)
            mono.StopCoroutine(routine);
            
        routine = mono.StartCoroutine(coroutine);

        return routine;
    }

    public static void RunRoutine(this MonoBehaviour mono, System.Collections.IEnumerator coroutine, string name)
    {
        if(mono == null) return;
        
        if(routines.TryGetValue((mono, name), out Coroutine routine))
        {
            if(routine != null) mono.StopCoroutine(routine);
        }
        else
        {
            routines.Add((mono, name), null);
        }

        routines[(mono, name)] = mono.StartCoroutine(coroutine);
    }

    public static Coroutine RunRoutine(this MonoBehaviour mono, System.Collections.IEnumerator coroutine)
    {
        return mono.StartCoroutine(coroutine);
    }

    public static void StopRoutine(this MonoBehaviour mono, System.Collections.IEnumerator coroutine)
    {
        mono.StopCoroutine(coroutine);
    }

    public static void StopRoutine(this MonoBehaviour mono, System.Collections.IEnumerator coroutine, string name)
    {
        if(routines.TryGetValue((mono, name), out Coroutine routine))
            mono.StopCoroutine(routine);
    }

    public static void StopRoutine(this MonoBehaviour mono, Coroutine routine)
    {
        mono.StopCoroutine(routine);
    }
}

public static class YieldUtil
{
    private readonly static Dictionary<float, WaitForSeconds> waitForSecondsList = new();
    private readonly static Dictionary<float, WaitForSecondsRealtime> waitForSecondsRealTimeList = new();
    public static WaitForSeconds WaitForSeconds(float time)
    {
        WaitForSeconds wfs;

        if(!waitForSecondsList.TryGetValue(time, out wfs))
            waitForSecondsList.Add(time, wfs = new WaitForSeconds(time));

        return wfs;
    }

    public static WaitForSecondsRealtime WaitForSecondsRealtime(float time)
    {
        WaitForSecondsRealtime wfs;

        if(!waitForSecondsRealTimeList.TryGetValue(time, out wfs))
            waitForSecondsRealTimeList.Add(time, wfs = new WaitForSecondsRealtime(time));

        return wfs;
    }
}

public static class BezierUtil
{
    public static Vector3 GetBezier_Vector3(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        t = Mathf.Clamp01(t);

        float oneMinus = 1f - t;
        
        Vector3 newVector3 = 
            oneMinus * oneMinus * p0 + 
            2f * oneMinus * t * p1 +
            t * t * p2;

        return newVector3;
    }
}

public static class RandomVector3
{
    public static Vector3 GetRandomVector3()
    {
        float randomAngle = UnityEngine.Random.Range(0f, 360f);

        return new Vector3(
            Mathf.Cos(randomAngle * Mathf.Deg2Rad),
            0f,
            Mathf.Sin(randomAngle * Mathf.Deg2Rad)
        );
    }
}

public static class InstanceFactory
{
    public static T GetInstance<T>(this object any, params object[] args)
    {
        return (T)Activator.CreateInstance(typeof(T), args);
    }
}

public class PoissonDiskSamplingVector
{
    private float radius = 10f, minDistance = 2f;
    private List<Vector3> usedPosition = new();
    private int maxAttempt = 10;

    public PoissonDiskSamplingVector(float radius, float minDistance = 2f)
    {
        this.radius = radius;
        this.minDistance = minDistance;
    }

    public Vector3 GetRandomRange(Vector3 centor)
    {
        for(int i = 0; i < maxAttempt; i++)
        {
            Vector2 randomPos = UnityEngine.Random.insideUnitCircle * radius;

            Vector3 candidatePos = centor + new Vector3(randomPos.x, 0f, randomPos.y);

            if(IsFarEnough(candidatePos))
            {
                usedPosition.Add(candidatePos);

                if(usedPosition.Count >= 5)
                    usedPosition.RemoveAt(0);
                
                return candidatePos;
            }
        }

        usedPosition.Clear(); 

        Vector2 resetPos = UnityEngine.Random.insideUnitCircle * radius; 
        return centor + new Vector3(resetPos.x, 0f, resetPos.y);
    }

    private bool IsFarEnough(Vector3 targetPos)
    {
        for(int i = 0; i < usedPosition.Count; i++)
        {
            float distance = (usedPosition[i] - targetPos).magnitude;
            if(distance <= minDistance)
                return false;
        }
        
        return true;
    }
}

public class ParabolaUtil
{
    public static Vector3 GetPrabola(Vector3 targetPos, Vector3 startPos, float angle)
    {
        Vector3 diff = targetPos - startPos;

        float horizontalDiff = new Vector2(diff.x, diff.z).magnitude;
        float verticalDiff = diff.y;

        float angleRad = angle * Mathf.Deg2Rad;
        float gravity = Mathf.Abs(Physics.gravity.y);

        //포물선 공식 역산해서 속도 얻기
        float velocity = Mathf.Sqrt(
            (gravity * horizontalDiff * horizontalDiff) /
            (2 * Mathf.Cos(angleRad) * Mathf.Cos(angleRad) *
                (Mathf.Tan(angleRad) * horizontalDiff - verticalDiff))
        );

        Vector3 horizontalDir = new Vector3(diff.x, 0f, diff.z).normalized;

        return (horizontalDir * velocity * Mathf.Cos(angleRad)) +
               (Vector3.up * velocity * Mathf.Sin(angleRad));
    }
}

public static class LifecycleBoundEvent
{
    public static CancellationTokenRegistration Subscribe<T>(
        Action<Action<T>> subscribe,
        Action<Action<T>> unsubscribe,
        Action<T> mainFunction, 
        MonoBehaviour monoBehaviour)
    {
        subscribe.Invoke(mainFunction);
        return monoBehaviour.destroyCancellationToken.Register(()=> { unsubscribe.Invoke(mainFunction); });
    }
}

public static class AnimationUtil
{
    public static float GetAnimationLength(this Animator animator, string clipName)
    {
        AnimationClip[] runtimeAnimator = animator.runtimeAnimatorController.animationClips;

        for(int i = 0; i < runtimeAnimator.Length; i++)
        {
            AnimationClip clip = runtimeAnimator[i];

            if(clip.name == clipName)
            {
                return clip.length;
            }
        }
        
        Util.Log("AnimationUtil.cs GetAnimationLength() Not Found Match Clip");
        return 0f;
    }

    public static AnimationClip GetAnimationClip(this Animator animator, string clipName)
    {
        AnimationClip[] runtimeAnimator = animator.runtimeAnimatorController.animationClips;

        for(int i = 0; i < runtimeAnimator.Length; i++)
        {
            AnimationClip clip = runtimeAnimator[i];

            if(clip.name == clipName)
            {
                return clip;
            }
        }
        
        Util.Log("AnimationUtil.cs GetAnimationClip() Not Found Match Clip");
        return null;
    }
}