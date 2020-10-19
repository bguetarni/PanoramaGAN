using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using UnityEngine.Rendering.PostProcessing;

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

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    string picturesPath;

    Vector3 initialAngle;
    Vector3 initialPosition;
    const int angleBetweenFrame = 30;
    const int panoramaAngle = 360;
    float rotatePerUpdate;
    float currentRotate;
    int angle;
    int offset;

    bool capturing_ground_truth;
    bool capturing_blur;


    void Start()
    {
        Screen.lockCursor = true;
        angle = angleBetweenFrame;
        GetComponent<Camera>().stereoSeparation = 0.064f; // Eye separation (IPD)
        picturesPath = Application.dataPath + "/Pictures/";
        rotatePerUpdate = Time.fixedDeltaTime * 360;
        currentRotate = 0;
        offset = 0;
        capturing_ground_truth = false;
        capturing_blur = false;
        initialAngle = transform.rotation.eulerAngles;
        initialPosition = transform.position;
    }

    void RenderCurrentImage(string filePath)
    {
        // create picture from screen screenshot
        var screenshot = ScreenCapture.CaptureScreenshotAsTexture();
        var bytes = screenshot.EncodeToJPG();

        // save picture
        System.IO.File.WriteAllBytes(filePath, bytes);
        //Debug.Log(Application.dataPath + "/Pictures/" + "render.jpg");

        // always destroy
        Object.Destroy(screenshot);
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
/*
        // render 320 images of current camera view
        if (Input.GetKeyDown(KeyCode.O))
        {
            RenderTexture cubemap = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32);
            cubemap.dimension = TextureDimension.Cube;
            RenderTexture equirect = new RenderTexture(4096, 2048, 24, RenderTextureFormat.ARGB32);

            RenderToCubemap(cubemap, 63, Camera.MonoOrStereoscopicEye.Mono);
            cubemap.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Mono);
            RenderTextureToJPG(equirect, picturesPath + "panorama/EquirectangularMono.jpg");

            RenderToCubemap(cubemap, 63, Camera.MonoOrStereoscopicEye.Left);
            cubemap.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Left);
            RenderTextureToJPG(equirect, picturesPath + "panorama/EquirectangularLeft.jpg");

            RenderToCubemap(cubemap, 63, Camera.MonoOrStereoscopicEye.Right);
            cubemap.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Right);
            RenderTextureToJPG(equirect, picturesPath + "panorama/EquirectangularRight.jpg");

            RenderToCubemap(cubemap);
            cubemap.ConvertToEquirect(equirect);
            RenderTextureToJPG(equirect, picturesPath + "panorama/Equirectangular.jpg");
        }
*/
        if (Input.GetKeyDown(KeyCode.V))
        {
            var zones = GameObject.FindGameObjectWithTag("Zones");
            for(int i=0; i < zones.transform.childCount; i++)
            {
                var cube = zones.transform.GetChild(i).gameObject;
                cube.GetComponent<Renderer>().enabled = false;
            }
            Debug.Log("Start MoveCamera Coroutine");
            StartCoroutine("MoveCamera");
        }
        
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("Stop MoveCamera Coroutine");
            StopCoroutine("MoveCamera");
            var zones = GameObject.FindGameObjectWithTag("Zones");
            for(int i=0; i < zones.transform.childCount; i++)
            {
                var cube = zones.transform.GetChild(i).gameObject;
                cube.GetComponent<Renderer>().enabled = true;
            }
        }
        
        // Launch groound truth acquisition
        if (Input.GetKeyDown(KeyCode.I))
        {
            if(!capturing_ground_truth)
            {
                Debug.Log("Genertating ground truth");
                GetComponent<PostProcessLayer>().SetMotion(false);
                transform.eulerAngles = initialAngle;
                transform.position = initialPosition;
                capturing_ground_truth = true;
            }
        }

        // One step of ground truth acquisistion
        if (capturing_ground_truth)
        {
            if (offset < panoramaAngle)
            {
                RenderCurrentImage(picturesPath + "ground truth/angle_" + offset.ToString() + ".jpg");
                offset += angleBetweenFrame;
                transform.eulerAngles = new Vector3(initialAngle.x, initialAngle.y + offset, initialAngle.z);
            }
            else
            {
                capturing_ground_truth = false;
                GetComponent<PostProcessLayer>().SetMotion(true);
                offset = 0;
            }
        }

        // Launch blurred images acquisition
        if (Input.GetKeyDown(KeyCode.U))
        {
            if(!capturing_blur)
            {
                Debug.Log("Generating blurred images");
                capturing_blur = true;
                initialAngle = transform.rotation.eulerAngles;
                initialPosition = transform.position;
            }
        }

        // Apply rotation and take a capturing_blur if current angle is adapted
        if (capturing_blur)
        {
            if (angle <= panoramaAngle)
            {
                if (currentRotate > angle)
                {
                    RenderCurrentImage(picturesPath + "blurred images/render_" + (angle - 45).ToString() + ".jpg");
                    angle += angleBetweenFrame;
                }

                // Apply rotation
                transform.Rotate(0, rotatePerUpdate, 0);
                currentRotate += rotatePerUpdate;
            }
            else
            {
                // Reset angle for new acquisition
                angle = angleBetweenFrame;
                capturing_blur = false;
                currentRotate = 0;
            }
        }
    }

    IEnumerator MoveCamera() 
    {
        while(true)
        { 
            transform.position = GetNewPosition();
            transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);
            yield return new WaitForSeconds(1f);
        }
    }

    Vector3 GetNewPosition()
    {
        var zones = GameObject.FindGameObjectWithTag("Zones");
        var cubeID = Random.Range(0, zones.transform.childCount-1);
        var cube = zones.transform.GetChild(cubeID).gameObject;
        var xPostion = cube.transform.position.x;
        var yPostion = cube.transform.position.y;
        var zPostion = cube.transform.position.z;
        
        var xScale = cube.transform.localScale.x;
        var yScale = cube.transform.localScale.y;
        var zScale = cube.transform.localScale.z;
        var x = xPostion + Random.Range(-xScale*0.5f, xScale*0.5f);
        var y = yPostion + Random.Range(-yScale*0.5f, yScale*0.5f);
        var z = zPostion + Random.Range(-zScale*0.5f, zScale*0.5f);
    /*
        Debug.Log("Cube " + cubeID.ToString());
        Debug.Log("X: " + xScale.ToString() + " / " + xDim.ToString());
        Debug.Log("Y: " + yScale.ToString() + " / " + yDim.ToString());
        Debug.Log("Z: " + zScale.ToString() + " / " + zDim.ToString());
    */

        return new Vector3(x, y, z);
    }
}