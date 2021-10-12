using System;
using System.Collections.Generic;
using UnityEngine;

namespace Crittr
{
    //Everytime that there's a "unused?" in the comments, it's because I can't see it in the dashboard
    //-Nextin
    [Serializable]
    public struct Device
    {
        //Name of the device
        public string name;
        //unused?
        public string family;
        //Motherboard (or computer model)
        public string model;
        //Type of device (Desktop, Laptop, etc.)
        public string type;
        //Name of the processor
        public string processor_type;
        //Amount of battery, if the device is a Desktop computer, it'll be set to -100
        public int battery_level;
        //The status of the battery at this moment (Charging, etc) [Could be replaced with an enum]
        public string battery_status;
        //Amount of RAM
        public int memory_size;
        //Orientation of the device
        public string orientation;
    }

    [Serializable]
    public struct GPU
    {
        //Name of the GPU
        public string name;
        //unused...? again?
        public string family;
        //unused... I guess. The model of the GPU is mentioned in the name.
        public string model;
        //unused...?
        public string type;
        //Renderer Version
        public string version;
        //GPU's Vendor ID
        public string vendor_id;
        //GPU's Vendor name, unused?
        public string vendor_name;
        //Amount of VRAM
        public int memory_size;

        public bool is_multi_threaded;
    }

    [Serializable]
    public struct OS
    {
        //OS Name/version
        public string name;
        //The type of OS (Windows, MacOS, Linux, etc..)
        public string family;
    }

    [Serializable]
    public class SysInfo
    {
        public Device device;
        public GPU gpu;
        public OS os;

        public SysInfo()
        {
            //Make a new device struct
            device = new Device();
            //Set all the info needed
            device.name = SystemInfo.deviceName;
            device.model = SystemInfo.deviceModel;
            device.type = SystemInfo.deviceType.ToString();
            device.processor_type = SystemInfo.processorType;
            device.battery_level = (int)Math.Floor(SystemInfo.batteryLevel) * 100;
            device.battery_status = SystemInfo.batteryStatus.ToString().ToLower();
            device.memory_size = SystemInfo.systemMemorySize;
            device.orientation = Input.deviceOrientation.ToString().ToLower();

            //Make a new GPU struct, set all of its needed info
            gpu = new GPU();
            gpu.name = SystemInfo.graphicsDeviceName;
            gpu.type = SystemInfo.graphicsDeviceType.ToString();
            gpu.vendor_id = SystemInfo.graphicsDeviceVendorID.ToString();
            gpu.vendor_name = SystemInfo.graphicsDeviceVendor;
            gpu.memory_size = SystemInfo.graphicsMemorySize;
            gpu.is_multi_threaded = SystemInfo.graphicsMultiThreaded;
            gpu.version = SystemInfo.graphicsDeviceVersion;

            //Make a new OS struct, set the info
            os = new OS();
            os.name = SystemInfo.operatingSystem;
            os.family = SystemInfo.operatingSystemFamily.ToString();
        }
    }

    [Serializable]
    public class AppInfo
    {
        public string version;
        public string env;
        public string platform;

        public AppInfo()
        {
            //Get application build version
            version = Application.version;
            //Set the environment to release by default
            env = "release";
            //If it's a Developer Build, set it to development instead.
            if (Debug.isDebugBuild)
            {
                env = "development";
            }
            //Get the platform of the app (e.g WindowsEditor)
            platform = Application.platform.ToString().ToLower();
        }
    }

    [Serializable]
    public struct User
    {
        public string email;
    }

    [Serializable]
    public class SDK
    {
        public string version = "0.1.0";
        public string platform = "unity";
    }

    [Serializable]
    public class Report
    {
        public SysInfo system;
        public AppInfo app;
        public List<string> logs;
        public User user;
        public SDK sdk;

        public string title = "";
        public string description = "";
        public string category = "bug";
        public string tags;
        public string extras;

        [NonSerialized]
        public List<Texture2D> screenshots;

        [NonSerialized]
        public List<string> attachments;

        [NonSerialized]
        private Dictionary<string, string> _tags = new Dictionary<string, string>();

        public Report()
        {
            //Make a new list of attachments
            attachments = new List<string>();
            //..and screenshots
            screenshots = new List<Texture2D>();
            //Initialize everything else
            system = new SysInfo();
            app = new AppInfo();
            user = new User();
            sdk = new SDK();
        }

        public void AddTag(string key, string value)
        {
            _tags.Add(key, value);
        }

        public void DeleteTag(string key)
        {
            _tags.Remove(key);
        }

        public void SetTags(Dictionary<string, string> newTags)
        {
            _tags = newTags;
        }

        public void SetExtras(Dictionary<string, object> _extras)
        {
            extras = CrittrUtils.Json.Serialize(_extras);
        }

        public void SetLogs(List<string> _logs)
        {
            logs = _logs;
        }

        public void SetUserEmail(string email)
        {
            user.email = email;
        }

        public string ToJson()
        {
            tags = CrittrUtils.Json.Serialize(_tags);
            return JsonUtility.ToJson(this);
        }
    }
}
