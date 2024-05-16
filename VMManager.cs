using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

public class VM
{
    public int Id { get; set; }
    public int mission_id { get; set; }
    public string vm_name { get; set; }
    public string vm_hostname { get; set; }
    public string vm_domain { get; set; }
    public string vm_os { get; set; } // Betriebssystem
    public string vm_ram { get; set; }
    public string vm_disk { get; set; }
    public string vm_cpu { get; set; }
    public string vm_datastore { get; set; }
    public string vm_datacenter { get; set; }
    public string vm_guest_id { get; set; }
    public string vm_creator { get; set; } // Ersteller
    public string vm_status { get; set; }
    public string created_at { get; set; } // Erstellt am
    public string updated_at { get; set; } // Modifiziert am
    public string vm_notes { get; set; } // Notizen
    public string mecm_id { get; set; }
    public bool updated { get; set; }
    public List<Interface> interfaces { get; set; }
    public List<Package> packages { get; set; }
    public List<Disk> Disks { get; set; }

    public VM()
    {
        interfaces = new List<Interface>();
        packages = new List<Package>();
        Disks = new List<Disk>();
    }

    // Tiefe Kopie-Methode für VM-Instanz
    public VM DeepClone()
    {
        VM clone = (VM)this.MemberwiseClone();
        clone.interfaces = new List<Interface>(this.interfaces.Select(i => i.Clone()));
        clone.packages = new List<Package>(this.packages.Select(p => p.Clone()));
        clone.Disks = new List<Disk>(this.Disks.Select(d => d.Clone()));
        return clone;
    }
}

public class Interface
{
    public int id { get; set; }
    public int vm_id { get; set; }
    public string ip { get; set; }
    public string subnet { get; set; }
    public string gateway { get; set; }
    public string dns1 { get; set; }
    public string dns2 { get; set; }
    public string vlan { get; set; }
    public string mac { get; set; }
    public string mode { get; set; }
    public string type { get; set; }

    public bool IsManagementInterface { get; set; } = false;

    public string DisplayText
    {
        get
        {
            if (IsManagementInterface)
            {
                return "Management Interface";
            }
            else if (mode == "DHCP")
            {
                return $"Mode: {mode}, VLAN: {vlan}";
            }
            else // Für "Static" oder andere Modi
            {
                return $"IP: {ip}, Mode: {mode}, VLAN: {vlan}";
            }
        }
    }

    // Interface-Attribute
    public Interface Clone()
    {
        return (Interface)this.MemberwiseClone();
    }
}
public class Disk
{
    public int Id { get; set; }
    public int vm_id { get; set; }
    public string disk_name { get; set; }
    public long disk_size { get; set; } // Größe der Festplatte in Gigabyte
    public string disk_type { get; set; } // Typ der Festplatte, z.B. SSD oder HDD

    public override string ToString()
    {
        // Erstellt den Display-String gemäß dem gewünschten Format
        return $"{disk_name} ({disk_size} GB, {disk_type})";
    }


    // Optional: Methode für tiefe Kopie
    public Disk Clone()
    {
        return (Disk)this.MemberwiseClone();
    }
}



public class Package
{
    public string id { get; set; }
    public string package_name { get; set; }
    public string package_version { get; set; }
    public string package_status { get; set; }

    // Package-Attribute
    public Package Clone()
    {
        return (Package)this.MemberwiseClone();
    }

}


public class VMManager
{

    // Methode zum
    // n der VMs

}
