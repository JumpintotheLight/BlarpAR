using System;
//
using Mirror;

public class SpawnMessage : MessageBase
{
	public bool isVrPlayer;

	public override void Deserialize (NetworkReader reader)
	{
		isVrPlayer = reader.ReadBoolean ();
	}

	public override void Serialize (NetworkWriter writer)
	{
		writer.Write (isVrPlayer);
	}
}
