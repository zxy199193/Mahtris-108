using UnityEngine;
using System.IO;
using System;

public class ScreenshotSubFolder : MonoBehaviour
{
    [Header("文件夹设置")]
    public string mainFolder = "Mahtris 108 Temp"; // 主文件夹，比如项目名
    public string subFolder = "Screenshot";      // 子文件夹，比如分类

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveScreenshot();
        }
    }

    void SaveScreenshot()
    {
        // 1. 获取桌面路径
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        // 2. 拼接完整路径：桌面 + 主文件夹 + 子文件夹
        // Path.Combine 可以接受多个参数，它会自动处理中间的斜杠
        string fullFolderPath = Path.Combine(desktopPath, mainFolder, subFolder);

        // 3. 智能创建文件夹 (如果主文件夹不存在，它会连主带子一起创建)
        if (!Directory.Exists(fullFolderPath))
        {
            Directory.CreateDirectory(fullFolderPath);
        }

        // 4. 生成文件名
        string fileName = "Img_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        string fullFilePath = Path.Combine(fullFolderPath, fileName);

        // 5. 保存
        ScreenCapture.CaptureScreenshot(fullFilePath);

        Debug.Log("截图已保存在子文件夹: " + fullFilePath);
    }
}