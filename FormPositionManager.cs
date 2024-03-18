using Newtonsoft.Json;
using System.IO;
using System.Windows.Forms;
using VirtuSphere;

public class FormPositionManager
{
    private const string FilePath = "formPositions.json";

    public static void SaveFormPosition(Form form)
    {
        FormPositions formPositions;
        if (File.Exists(FilePath))
        {
            var content = File.ReadAllText(FilePath);
            formPositions = JsonConvert.DeserializeObject<FormPositions>(content) ?? new FormPositions();
        }
        else
        {
            formPositions = new FormPositions();
        }

        formPositions.Positions[form.Name] = form.Location;
        var json = JsonConvert.SerializeObject(formPositions, Formatting.Indented);
        File.WriteAllText(FilePath, json);
    }

    public static void LoadFormPosition(Form form)
    {
        if (File.Exists(FilePath))
        {
            var content = File.ReadAllText(FilePath);
            var formPositions = JsonConvert.DeserializeObject<FormPositions>(content);
            if (formPositions != null && formPositions.Positions.ContainsKey(form.Name))
            {
                form.Location = formPositions.Positions[form.Name];
            }
        }
        else
        {
            MessageBox.Show("File not found: "+FilePath);

        }
    }
}
