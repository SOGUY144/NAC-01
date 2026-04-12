using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using SOGUY.CallSystem.Data;
using System.Text.RegularExpressions;

namespace SOGUY.CallSystem.EditorTools
{
    public class DialogueCSVImporter : EditorWindow
    {
        private TextAsset csvFile;
        private string saveFolder = "Assets/SOGUY/ScriptableObjects/GeneratedDialogues";

        [MenuItem("SOGUY Tools/Dialogue CSV Importer")]
        public static void ShowWindow()
        {
            GetWindow<DialogueCSVImporter>("Dialogue CSV Importer");
        }

        private void OnGUI()
        {
            GUILayout.Label("✨ เครื่องมือเสกเนื้อเรื่องจาก Excel แบบรวดเดียว ✨", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("ไฟล์ CSV ต้องเรียงหัวคอลัมน์จากซ้ายไปขวาดังนี้:\n[ID] | [CallerText] | [Delay] | [IsTerminal] | [Outcome] | [IsAutoProceed] | [AutoNextID] | [Choice1Text] | [C1_NextID] | [Choice2Text] | [C2_NextID] | [Choice3Text] | [C3_NextID] | [SpeakerName] | [TimerDuration(วิ)] | [Timeout_NextID]", MessageType.Info);

            GUILayout.Space(10);
            csvFile = (TextAsset)EditorGUILayout.ObjectField("วางไฟล์ .csv ตรงนี้:", csvFile, typeof(TextAsset), false);
            
            GUILayout.Space(5);
            saveFolder = EditorGUILayout.TextField("โฟลเดอร์สำหรับเซฟ:", saveFolder);

            GUILayout.Space(15);
            if (GUILayout.Button("🔥 กดปุ่มนี้เพื่อเสกไฟล์ทั้งหมด (1 วินาที) 🔥", GUILayout.Height(40)))
            {
                if (csvFile != null)
                {
                    string path = AssetDatabase.GetAssetPath(csvFile);
                    
                    // อ่านไฟล์แบบทะลุเกราะป้องกันของโปรแกรมอื่น (กันบัค Sharing Violation จาก Excel)
                    using (var fs = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                    using (var sr = new System.IO.StreamReader(fs, System.Text.Encoding.UTF8))
                    {
                        string rawText = sr.ReadToEnd();
                        ProcessCSV(rawText);
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "ยังไม่ได้ลากไฟล์ CSV ใส่ช่องเลยครับลูกพี่!", "OK");
                }
            }
        }

        private void ProcessCSV(string csvContent)
        {
            if (csvFile.name.Contains("Importer"))
            {
                EditorUtility.DisplayDialog("ผิดไฟล์ครับลูกพี่!", "คุณเผลอลากไฟล์สคริปต์ C# (.cs) มาใส่ครับ!\nต้องลากไฟล์ประวัติเนื้อเรื่อง (.csv) มาใส่เท่านั้นครับ", "อ๊ะ โทษที");
                return;
            }

            if (!AssetDatabase.IsValidFolder(saveFolder))
            {
                // บังคับสร้างโฟลเดอร์ถ้าไม่มี
                string[] folders = saveFolder.Split('/');
                string currentPath = folders[0];
                for (int i = 1; i < folders.Length; i++)
                {
                    if (!AssetDatabase.IsValidFolder(currentPath + "/" + folders[i]))
                    {
                        AssetDatabase.CreateFolder(currentPath, folders[i]);
                    }
                    currentPath += "/" + folders[i];
                }
            }

            // แยกบรรทัด
            string[] lines = csvContent.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length <= 1) return; // มีแต่หัวข้อ หรือว่างเปล่า

            Dictionary<string, DialogueNode> nodeDict = new Dictionary<string, DialogueNode>();
            
            try
            {
                EditorUtility.DisplayProgressBar("CSV Importer", "กำลังสร้างไฟล์ทีละอัน...", 0f);

                // รอบที่ 1: สร้างก้อนไฟล์เปล่าๆ ทั้งหมดขึ้นมาก่อน
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] cols = ParseCSVLine(lines[i]);
                    if (cols.Length < 1 || string.IsNullOrEmpty(cols[0])) continue;

                    string id = cols[0].Trim();
                    
                    // ป้องกันชื่อไฟล์ประหลาด (เช่นเผลอเอาโค้ด C# มาใส่)
                    if (id.Contains("class ") || id.Contains("namespace "))
                    {
                        throw new System.Exception("ข้อมูลผิดปกติ! คุณน่าจะลากไฟล์โค้ด C# ผสมเข้ามาแทนที่จะเป็นตารางครับ");
                    }

                    string path = $"{saveFolder}/{id}.asset";

                    DialogueNode node = AssetDatabase.LoadAssetAtPath<DialogueNode>(path);
                    if (node == null)
                    {
                        node = ScriptableObject.CreateInstance<DialogueNode>();
                        AssetDatabase.CreateAsset(node, path);
                        AssetDatabase.SaveAssets(); // บังคับเซฟลงดิสก์ทันที เพื่อกัน Object ล่องหน
                    }

                    // กรอกข้อมูลโดยตรง (ปลอดภัยสุดสำหรับไฟล์เกิดใหม่)
                    string cText = GetCol(cols, 1);
                    node.CallerText = cText.Replace("\\n", "\n"); // รองรับเว้นบรรทัด
                    node.SpeakerName = GetCol(cols, 13); // รับชื่อคนพูดจากคอลัมน์ 14 เพื่อเปลี่ยนคนพูดกลางสาย
                    
                    float delay;
                    if (float.TryParse(GetCol(cols, 2), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out delay))
                    {
                        node.DelayAfterAudio = delay;
                    }
                    
                    float qteTimer;
                    if (float.TryParse(GetCol(cols, 14), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out qteTimer))
                    {
                        node.TimerDuration = qteTimer;
                    }
                    else node.TimerDuration = 0f;

                    node.IsTerminalNode = GetCol(cols, 3).ToUpper() == "TRUE";
                    
                    SOGUY.CallSystem.Data.CallOutcome outcome;
                    if (System.Enum.TryParse(GetCol(cols, 4), out outcome))
                    {
                        node.Outcome = outcome;
                    }

                    node.IsAutoProceed = GetCol(cols, 5).ToUpper() == "TRUE";
                    node.Choices.Clear();

                    EditorUtility.SetDirty(node);
                    nodeDict[id] = node;
                    
                    // บันทึก Log เพื่อพิสูจน์ว่าข้อความถูกดึงมาจริงๆ
                    Debug.Log($"เสกไฟล์ [ {id} ] สำเร็จ! \nข้อความคนโทร: '{GetCol(cols, 1)}'");
                }

                // รอบที่ 2: ผูกลูกศรเอาแต่ละไฟล์มาเชื่อมหากัน!!
                EditorUtility.DisplayProgressBar("CSV Importer", "กำลังโยงสายใยแมงมุมเข้าหากัน...", 0.5f);
                
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] cols = ParseCSVLine(lines[i]);
                    if (cols.Length < 1 || string.IsNullOrEmpty(cols[0])) continue;

                    string id = cols[0].Trim();
                    if (!nodeDict.ContainsKey(id)) continue;

                    DialogueNode node = nodeDict[id];

                    // เชื่อมสาย Auto
                    string autoNextID = GetCol(cols, 6);
                    if (!string.IsNullOrEmpty(autoNextID) && nodeDict.ContainsKey(autoNextID))
                    {
                        node.AutoNextNode = nodeDict[autoNextID];
                    }

                    // เชื่อมทางเลือกที่ 1
                    string c1Text = GetCol(cols, 7);
                    if (!string.IsNullOrEmpty(c1Text))
                    {
                        Choice c = new Choice { ChoiceText = c1Text };
                        string c1Next = GetCol(cols, 8);
                        if (!string.IsNullOrEmpty(c1Next) && nodeDict.ContainsKey(c1Next)) c.NextNode = nodeDict[c1Next];
                        node.Choices.Add(c);
                    }

                    // เชื่อมทางเลือกที่ 2
                    string c2Text = GetCol(cols, 9);
                    if (!string.IsNullOrEmpty(c2Text))
                    {
                        Choice c = new Choice { ChoiceText = c2Text };
                        string c2Next = GetCol(cols, 10);
                        if (!string.IsNullOrEmpty(c2Next) && nodeDict.ContainsKey(c2Next)) c.NextNode = nodeDict[c2Next];
                        node.Choices.Add(c);
                    }

                    // เชื่อมทางเลือกที่ 3
                    string c3Text = GetCol(cols, 11);
                    if (!string.IsNullOrEmpty(c3Text))
                    {
                        Choice c = new Choice { ChoiceText = c3Text };
                        string c3Next = GetCol(cols, 12);
                        if (!string.IsNullOrEmpty(c3Next) && nodeDict.ContainsKey(c3Next)) c.NextNode = nodeDict[c3Next];
                        node.Choices.Add(c);
                    }
                    
                    // ปลายทางฉากจบเวรเมื่อ Timeout (คอลัมน์ 16)
                    string timeoutNext = GetCol(cols, 15);
                    if (!string.IsNullOrEmpty(timeoutNext) && nodeDict.ContainsKey(timeoutNext))
                    {
                        node.TimeoutNode = nodeDict[timeoutNext];
                    }
                    else node.TimeoutNode = null;

                    EditorUtility.SetDirty(node);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                EditorUtility.DisplayDialog("เสกเสร็จสมบูรณ์!!", $"สร้างเนื้อเรื่องและผูกลูกศรให้ทั้งหมดจำนวน {nodeDict.Count} ไฟล์ เรียบร้อยแล้ว!\nเข้าไปดูในโฟลเดอร์ {saveFolder} ได้เลยครับ!", "สุดยอดไปเลยลูกพี่");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Importer Error] {e.Message}");
                EditorUtility.DisplayDialog("เจอข้อผิดพลาด!", e.Message, "เข้าใจแล้ว");
            }
            finally
            {
                // บังคับเคลียร์หลอดโหลดทิ้งเสมอ ไม่ว่าจะสำเร็จหรือพังก็ตาม Unity จะได้ไม่ค้าง!
                EditorUtility.ClearProgressBar();
            }
        }

        private string GetCol(string[] cols, int index)
        {
            if (index < cols.Length)
            {
                string val = cols[index].Trim();
                if (val.StartsWith("\"") && val.EndsWith("\""))
                {
                    val = val.Substring(1, val.Length - 2).Replace("\"\"", "\"");
                }
                return val;
            }
            return "";
        }

        // ตัวอ่านท่อน CSV แบบลูปเช็คทีละอักษร (แม่นยำ 100% ป้องกัน Regex บัคภาษาไทย)
        private string[] ParseCSVLine(string line)
        {
            char delimiter = ','; // บังคับใช้ลูกน้ำเท่านั้น ห้ามเดาเอาเองเด็ดขาด
            List<string> result = new List<string>();
            bool inQuotes = false;
            string current = "";

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '\"')
                {
                    inQuotes = !inQuotes; // สลับสถานะเปิด/ปิดฟันหนู
                }
                else if (c == delimiter && !inQuotes)
                {
                    result.Add(current); // จบช่องตาราง
                    current = "";
                }
                else
                {
                    current += c; // สะสมอักษร
                }
            }
            result.Add(current); // แอคคอมลัมน์สุดท้าย
            return result.ToArray();
        }
    }
}
