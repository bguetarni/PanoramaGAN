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

    float rotatePerUpdate;
    Vector3 initialAngle;
    int angle;
    float currentRotate = 0;
    int angleBetweenFrame = 45;
    int angleBetweenGroundTruth = 40;
    Camera camera;
    bool shot = true;
    string picturesPath;
    bool capturing = false;
    float offset = 0;


    void Start()
    {
        Screen.lockCursor = true;
        initialAngle = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Transform>().rotation.eulerAngles;
        angle = angleBetweenFrame;
        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        camera.stereoSeparation = 0.064f; // Eye separation (IPD) of 64mm.
        picturesPath = Application.dataPath + "/Pictures/";
        rotatePerUpdate = Time.fixedDeltaTime * 360;
    }

    void RenderCurrentImage(string filePath)
    {
        // create picture from screen screenshot
        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
        byte[] bytes = screenshot.EncodeToJPG();

        // save picture
        System.IO.File.WriteAllBytes(filePath, bytes);
        //Debug.Log(Application.dataPath + "/Pictures/" + "render.jpg");

        // always destroy
        Object.Destroy(screenshot);
    }

    void RenderTextureToJPG(RenderTexture renderTexture, string file)
    {
        RenderTexture currenRT = RenderTexture.active;
        RenderTexture.active = renderTexture;
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height);
        texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        texture.Apply();
        byte[] bytes = texture.EncodeToJPG();
        System.IO.File.WriteAllBytes(file, bytes);
        RenderTexture.active = currenRT;
    }

    void Update()
    {


        //rotationX += Input.GetAxis("Mouse X") * cameraSensitivity * Time.deltaTime;
        //rotationY += Input.GetAxis("Mouse Y") * cameraSensitivity * Time.deltaTime;
        //rotationY = Mathf.Clamp(rotationY, -90, 90);

        //transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
        //transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);

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

        // Render the camera view
        if (Input.GetKey(KeyCode.P))
        {
            RenderCurrentImage(picturesPath + "render.jpg");
        }

        //TODO: disable blur motion during acquisition and enable it back after
        // Launch groound truth acquisition
        if (Input.GetKey(KeyCode.G))
        {
            Debug.Log("Genertating ground truth");
            GetComponent<PostProcessLayer>().SetMotion(false);
            transform.eulerAngles = initialAngle;
            capturing = true;
        }
        // One step of ground truth acquisistion
        if (capturing)
        {
            if(offset < 360)
            {
                RenderCurrentImage(picturesPath + "ground truth/angle_" + offset.ToString() + ".jpg");
                transform.eulerAngles = new Vector3(initialAngle.x, initialAngle.y + offset, initialAngle.z);
                offset += angleBetweenGroundTruth;
            }
            else
            { 
                capturing = false;
                GetComponent<PostProcessLayer>().SetMotion(true);
            }
        }
        
        // render 320 images of current camera view
        if (Input.GetKey(KeyCode.R))
        {
            RenderTexture cubemap = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32);
            cubemap.dimension = TextureDimension.Cube;
            RenderTexture equirect = new RenderTexture(4096, 2048, 24, RenderTextureFormat.ARGB32);

            camera.RenderToCubemap(cubemap, 63, Camera.MonoOrStereoscopicEye.Mono);
            cubemap.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Mono);
            RenderTextureToJPG(equirect, picturesPath + "panorama/EquirectangularMono.jpg");

            camera.RenderToCubemap(cubemap, 63, Camera.MonoOrStereoscopicEye.Left);
            cubemap.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Left);
            RenderTextureToJPG(equirect, picturesPath + "panorama/EquirectangularLeft.jpg");

            camera.RenderToCubemap(cubemap, 63, Camera.MonoOrStereoscopicEye.Right);
            cubemap.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Right);
            RenderTextureToJPG(equirect, picturesPath + "panorama/EquirectangularRight.jpg");

            camera.RenderToCubemap(cubemap);
            cubemap.ConvertToEquirect(equirect);
            RenderTextureToJPG(equirect, picturesPath + "panorama/Equirectangular.jpg");
        }

        // stop the rotation
        if (Input.GetKey(KeyCode.S)) { shot = false; }

        // Apply rotation and take a shot if current angle is adapted
        if (shot)
        {
            //transform.localRotation *= Quaternion.AngleAxis(rotatePerUpdate * cameraSensitivity * Time.deltaTime, Vector3.up);
            transform.Rotate(0, rotatePerUpdate, 0);
            currentRotate += rotatePerUpdate;
            if (angle <= 360)
            {
                if (currentRotate > angle)
                {
                    Debug.Log("angle: " + angle.ToString());
                    RenderCurrentImage(picturesPath + "blurred images/render_" + angle.ToString() + ".jpg");
                    angle += angleBetweenFrame;
                }
            }
            else { shot = false; }
        }
    }
}