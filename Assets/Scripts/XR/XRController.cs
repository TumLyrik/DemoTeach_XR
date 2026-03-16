/*
 * 此脚本保证unity在play模式下自动启动Mixed Reality OpenXR，停止play后自动关闭OpenXR
 * 没有此脚本OpenXR不能正常关闭，再次play后会出现报错提示已经打开了OpenXR
 * unity Project Setting/XR Plug in Management中需要initialze XR on Startup以保证QRkos正常工作
*/

using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;

public class XRController : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(StartXR());
    }

    private void OnDisable()
    {
        StopXR();
    }

    private IEnumerator StartXR()
    {
        if (!XRGeneralSettings.Instance.Manager.isInitializationComplete)
        {
            yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

            if (XRGeneralSettings.Instance.Manager.activeLoader != null)
            {
                XRGeneralSettings.Instance.Manager.StartSubsystems();
            }
        }
    }

    private void StopXR()
    {
        if (XRGeneralSettings.Instance.Manager.activeLoader != null)
        {
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        }
    }
}
