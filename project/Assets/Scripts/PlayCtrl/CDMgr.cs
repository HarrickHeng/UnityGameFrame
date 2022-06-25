using System.Collections.Generic;
using UnityEngine;

public class CDMgr : MonoSingleton<CDMgr>
{
    Dictionary<string, float> dic = new Dictionary<string, float>(); //保存id和cd时长信息
    Dictionary<string, float> cds = new Dictionary<string, float>(); //保存当前的cd
    List<string> ids = new List<string>(); //保存id

    void Update()
    {
        //冷却计时
        if (ids.Count > 0)
        {
            ids.ForEach(
                _ =>
                {
                    if (cds[_] > 0)
                    {
                        cds[_] -= Time.deltaTime;
                    }
                    else
                    {
                        cds[_] = 0;
                    }
                }
            );
        }
    }

    //第一个参数传字符串也可，这里为了方便传参用的枚举
    //第二个参数为cd时长
    public void AddCD(string id, float time)
    {
        if (dic.ContainsKey(id))
        {
            Debug.LogError("该ID已存在！");
            return;
        }
        dic.Add(id, time);
        cds.Add(id, 0); //一开始没进入冷却
        ids.Add(id);
    }

    //进入冷却
    void StartCal(string id)
    {
        if (!dic.ContainsKey(id))
        {
            Debug.LogError("不存在该ID！");
            return;
        }

        cds[id] = dic[id];
    }

    //是否冷却结束，如果冷却结束会返回true并重新开始计算cd
    public bool IsReady(string id)
    {
        if (cds.ContainsKey(id))
        {
            if (cds[id] <= 0)
            {
                StartCal(id);
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            Debug.LogError("不存在该ID！");
            return false;
        }
    }

    //获取当前CD
    public float GetCurrentCD(string id)
    {
        if (cds.ContainsKey(id))
        {
            return cds[id];
        }
        else
        {
            Debug.LogError("不存在该ID！");
            return 9999999;
        }
    }

    //获取CD时长
    public float GetWholeCD(string id)
    {
        if (dic.ContainsKey(id))
        {
            return dic[id];
        }
        else
        {
            Debug.LogError("不存在该ID！");
            return 9999999;
        }
    }

    //移除id
    public void RemoveCD(string id)
    {
        if (dic.ContainsKey(id))
        {
            dic.Remove(id);
            cds.Remove(id);
            ids.Remove(id);
        }
        else
        {
            Debug.LogError("不存在该ID！");
        }
    }
}
