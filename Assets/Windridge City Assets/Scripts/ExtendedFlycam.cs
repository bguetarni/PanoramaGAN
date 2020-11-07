using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;
using Diagnostics = System.Diagnostics;
using System.IO;

public class ExtendedFlycam : MonoBehaviour
{

    /*
	EXTENDED FLYCAM
		Desi Quintans (CowfaceGames.com), 17 August 2012.
		Based on FlyThrough.js by Slin (http://wiki.unity3d.com/index.php/FlyThrough), 17 May 2011.
 
	LICENSE
		Free as in speech, and free as in beer.
 
	FEATURES
		WASD/Arrows:    Movement
		          Q:    Climb
		          E:    Drop
                      Shift:    Move faster
                    Control:    Move slower
                        End:    Toggle cursor locking to screen (you can also press Ctrl+P to toggle play mode on and off).
	*/

    public float cameraSensitivity = 90;
    public float climbSpeed = 4;
    public float normalMoveSpeed = 10;
    public float slowMoveFactor = 0.25f;
    public float fastMoveFactor = 3;

    string picturesPath;
    string datasetPath;

    Vector3 initialAngle;
    Vector3 initialPosition;
    const int angleBetweenFrame = 18;
    float rotatePerUpdate;
    bool is_camera_moving;


    void Start()
    {
        Screen.lockCursor = true;
        GetComponent<Camera>().stereoSeparation = 0.064f; // Eye separation (IPD)
        picturesPath = Application.dataPath + "/../Pictures/";
        datasetPath = Application.dataPath + "/../data/";
        rotatePerUpdate = Time.fixedDeltaTime * 360;
        Debug.Log("Rotate per update: " + rotatePerUpdate.ToString() + "°");
        initialAngle = transform.rotation.eulerAngles;
        initialPosition = transform.position;
    }

    void RenderCurrentImage(string filePath)
    {
        ScreenCapture.CaptureScreenshot(filePath);
    }

    void RenderTextureToJPG(RenderTexture renderTexture, string file)
    {
        var currenRT = RenderTexture.active;
        RenderTexture.active = renderTexture;
        var texture = new Texture2D(renderTexture.width, renderTexture.height);
        texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        texture.Apply();
        var bytes = texture.EncodeToJPG();
        System.IO.File.WriteAllBytes(file, bytes);
        RenderTexture.active = currenRT;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            transform.position += transform.forward * (normalMoveSpeed * fastMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
            transform.position += transform.right * (normalMoveSpeed * fastMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            transform.position += transform.forward * (normalMoveSpeed * slowMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
            transform.position += transform.right * (normalMoveSpeed * slowMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
        }
        else
        {
            transform.position += transform.forward * normalMoveSpeed * Input.GetAxis("Vertical") * Time.deltaTime;
            transform.position += transform.right * normalMoveSpeed * Input.GetAxis("Horizontal") * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.Q)) { transform.position += transform.up * climbSpeed * Time.deltaTime; }
        if (Input.GetKey(KeyCode.E)) { transform.position -= transform.up * climbSpeed * Time.deltaTime; }

        if (Input.GetKeyDown(KeyCode.End))
        {
            if (Cursor.lockState == CursorLockMode.None)
                Cursor.lockState = CursorLockMode.Locked;
            else
                Cursor.lockState = CursorLockMode.None;
        }

        // render 320 images of current camera view
        if (Input.GetKeyDown(KeyCode.O))
        {
            RenderTexture cubemap = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32);
            cubemap.dimension = TextureDimension.Cube;
            RenderTexture equirect = new RenderTexture(4096, 2048, 24, RenderTextureFormat.ARGB32);

            GetComponent<Camera>().RenderToCubemap(cubemap, 63, Camera.MonoOrStereoscopicEye.Mono);
            cubemap.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Mono);
            RenderTextureToJPG(equirect, picturesPath + "panorama/EquirectangularMono.jpg");

            GetComponent<Camera>().RenderToCubemap(cubemap, 63, Camera.MonoOrStereoscopicEye.Left);
            cubemap.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Left);
            RenderTextureToJPG(equirect, picturesPath + "panorama/EquirectangularLeft.jpg");

            GetComponent<Camera>().RenderToCubemap(cubemap, 63, Camera.MonoOrStereoscopicEye.Right);
            cubemap.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Right);
            RenderTextureToJPG(equirect, picturesPath + "panorama/EquirectangularRight.jpg");

            GetComponent<Camera>().RenderToCubemap(cubemap);
            cubemap.ConvertToEquirect(equirect);
            RenderTextureToJPG(equirect, picturesPath + "panorama/Equirectangular.jpg");
        }

        // Start the movement of camera
        if (Input.GetKeyDown(KeyCode.V))
        {
            if(!is_camera_moving)
            {
                Debug.Log("Start MoveCamera Coroutine");
                StartCoroutine("MoveCamera");
            }
            else
            {
                Debug.Log("MoveCamera coroutine is already running");
            }
        }
        
        // Stop the movement of camera
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("Stop MoveCamera Coroutine");
            StopCoroutine("MoveCamera");
            is_camera_moving = false;
        }
        
        // Launch blurred images acquisition
        if (Input.GetKeyDown(KeyCode.U))
        {
            StartCoroutine("GenerateBlurredImages", picturesPath);
        }

        // Launch groound truth acquisition
        if (Input.GetKeyDown(KeyCode.I))
        {
            StartCoroutine("GenerateGroundTruth", picturesPath);
        }

        // Launch dataset generation
        if (Input.GetKeyDown(KeyCode.D))
        {
            StartCoroutine("DatasetGenerate", 5);
        }
    }

    IEnumerator GenerateBlurredImages(string path)
    {
        // save initial values for ground-truth
        initialPosition = transform.position;
        initialAngle = transform.eulerAngles;
        
        // var sw = new Diagnostics.Stopwatch();
        // sw.Start();
        var angle = 0.0f;
        var frameNumber = 1;
        for (float currentRotate = rotatePerUpdate; currentRotate <= 360.0f; currentRotate += rotatePerUpdate) 
        {
            if(currentRotate > angle)
            {
                var pictureName = frameNumber.ToString("D2") + ".png";
                // Debug.Log("Saving blurred " + pictureName);
                RenderCurrentImage(path + "blurred/" + pictureName);
                frameNumber++;
                angle += angleBetweenFrame;
            }
            transform.Rotate(0, rotatePerUpdate, 0);
            yield return null;
        }
        // sw.Stop();
        // Debug.Log("Elapsed time for blurred images " + sw.Elapsed.ToString() + "s");
    }

    IEnumerator GenerateGroundTruth(string path)
    {
        /*
            Command line for generating pano from directory of images :
                'ls -d images/* | xargs ./image-stitching'
                    with:
                        images: directory containing the images
                        image-stitching: algo executable
                        xargs: UNIX command line to expand a list of arguments
        */
        GetComponent<PostProcessLayer>().SetMotion(false);
        transform.position = initialPosition;
        transform.eulerAngles = initialAngle;
        
        var frameNumber = 1;
        for (int offset = angleBetweenFrame; offset <= 360; offset += angleBetweenFrame) 
        {
            var pictureName = frameNumber.ToString("D2") + ".png";
            // Debug.Log("Saving ground-truth " + pictureName);
            RenderCurrentImage(path + "ground_truth/" + pictureName);
            frameNumber++;
            transform.eulerAngles = new Vector3(initialAngle.x, initialAngle.y + offset, initialAngle.z);
            yield return null;
        }
        
        GetComponent<PostProcessLayer>().SetMotion(true);
    }

    IEnumerator MoveCamera() 
    {
        is_camera_moving = true;
        foreach (var cube in GameObject.FindGameObjectsWithTag("Zones"))
        {
            cube.GetComponent<Renderer>().enabled = false;
        }
        while(true)
        { 
            transform.position = GetNewPosition();
            transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);
            yield return new WaitForSeconds(1f);
        }
    }

    Vector3 GetNewPosition()
    {
        var cubesList = GameObject.FindGameObjectsWithTag("Zones");
        var cubeID = Random.Range(0, cubesList.Length-1);
        var cube = cubesList[cubeID];
        var xPostion = cube.transform.position.x;
        var yPostion = cube.transform.position.y;
        var zPostion = cube.transform.position.z;
        var xScale = cube.transform.localScale.x;
        var yScale = cube.transform.localScale.y;
        var zScale = cube.transform.localScale.z;

        var p = new Vector3(xPostion, yPostion, zPostion);
        p += cube.transform.right*Random.Range(-xScale*0.5f, xScale*0.5f);
        p += cube.transform.up*Random.Range(-yScale*0.5f, yScale*0.5f);
        p += cube.transform.forward*Random.Range(-zScale*0.5f, zScale*0.5f);
        return p;
    }

    IEnumerator DatasetGenerate(int nbSamples)
    {
        foreach (var cube in GameObject.FindGameObjectsWithTag("Zones"))
        {
            cube.GetComponent<Renderer>().enabled = false;
        }
        for(var i=0; i<nbSamples; i++)
        {
            // sample name in the form of: 00000XXX
            var directoryName = (i+1).ToString("D" + nbSamples.ToString().Length.ToString()) + "/";
            Debug.Log("sample n° " + directoryName);
            
            // create directory with the name of the sample if doesn't exist
            if(!Directory.Exists(datasetPath + directoryName))
            {
                Directory.CreateDirectory(datasetPath + directoryName);
                Directory.CreateDirectory(datasetPath + directoryName + "blurred/");
                Directory.CreateDirectory(datasetPath + directoryName + "ground_truth/");
            }

            // new position and angle for camera
            transform.position = GetNewPosition();
            transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);
            yield return null;
            
            // render blurred, wait and render ground-truth
            yield return StartCoroutine("GenerateBlurredImages", datasetPath + directoryName);
            yield return StartCoroutine("GenerateGroundTruth", datasetPath + directoryName);
        }
    }
}