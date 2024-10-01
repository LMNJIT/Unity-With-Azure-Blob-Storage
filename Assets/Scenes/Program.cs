using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

public class Program : MonoBehaviour
{
    private string connectionString;
    private string blobName;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Script runs when cube is rendered.");

        GameObject ifieldobj = GameObject.Find("Canvas/param");
        InputField ifield = ifieldobj.GetComponent<InputField>();
        ifield.onEndEdit.AddListener(OnConnectionStringEntered);

        GameObject ifieldobj2 = GameObject.Find("Canvas/param2");
        InputField ifield2 = ifieldobj2.GetComponent<InputField>();
        ifield2.onEndEdit.AddListener(OnBlobNameEntered);
    }

    //User enters connection string and hits Return key.
    public void OnConnectionStringEntered(string cxnstr)
    {
        Debug.Log("Connection String Entered: " + cxnstr);
        connectionString = cxnstr;

        //Wait for object name and connection string to be entered before proceeding
        if (!string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(blobName))
        {
            ParameterInput(connectionString, blobName);
        }
    }

    //User enters blob string and hits Return key
    public void OnBlobNameEntered(string blob)
    {
        Debug.Log("Blob Name Entered: " + blob);
        blobName = blob;

        //Wait for object name and connection string to be entered before proceeding
        if (!string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(blobName))
        {
            ParameterInput(connectionString, blobName);
        }
    }

    //Handle the inputs
    public void ParameterInput(string cxnstr, string blob)
    {
        Debug.Log("Using Connection String: " + cxnstr);
        Debug.Log("Using Blob Name: " + blob);

        CloudStorageAccount act = CloudStorageAccount.Parse(cxnstr);
        CloudBlobClient client = act.CreateCloudBlobClient();

        var container = client.GetContainerReference("example");
        container.CreateIfNotExistsAsync().Wait();

        CloudBlockBlob cloudBlob = container.GetBlockBlobReference("log.txt");
        appendText(cloudBlob, "Unity log: " + System.DateTime.UtcNow.ToString("MM-dd-yyyy hh:mm:ss"));

        // Call downloadDemo with the blob name
        downloadDemo(cxnstr, blob);
    }

    //Search Azure storage for given blobName (object file) and instantiate it
    public async Task downloadDemo(string cxnstr, string blobName)
    {
        BlobModel bm = new BlobModel(blobName, "example", cxnstr);
        if (await bm.exists())
        {
            await bm.download(blobName); 
            Debug.Log("Downloaded " + blobName);

            Mesh meshHold = new Mesh();
            ObjImporter newMesh = new ObjImporter();
            meshHold = newMesh.ImportFile("./Assets/Resources/" + blobName); 
            Debug.Log("Imported " + blobName);

            GameObject myObject = new GameObject();
            MeshRenderer meshRenderer = myObject.AddComponent<MeshRenderer>();
            MeshFilter filter = myObject.AddComponent<MeshFilter>();
            filter.mesh = meshHold;

            // Load material
            Material objectMaterial = Resources.Load("metal01", typeof(Material)) as Material;
            myObject.GetComponent<MeshRenderer>().material = objectMaterial;

            Instantiate(myObject);
            myObject.transform.position = new Vector3(47, -365, -59);

            Debug.Log("Done");
        }
    }

    public static async Task appendText(CloudBlockBlob blob, string v)
    {
        var upload = v;

        if (await blob.ExistsAsync())
        {
            var content = await blob.DownloadTextAsync();
            upload = content + "\n" + v;
        }

        await blob.UploadTextAsync(upload);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}