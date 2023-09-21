using UnityEngine;
using Mirror;

public class InstanceWriter : NetworkWriter
{
    public static void WriteInstance(NetworkWriter writer, Instance instance)
    {
        writer.WriteInt(instance.instanceId);
    }
}

public static class InstanceWriterExtension
{
    public static void WriteInstance(this NetworkWriter writer, Instance instance)
    {
        InstanceWriter.WriteInstance(writer, instance);
    }
}