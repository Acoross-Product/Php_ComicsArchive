<?php
//Filename: IConnectInfo.php
interface IConnectInfo
{
	const HOST ="xxxxx";
	const UNAME ="xxxxx";
	const PW ="xxxxx";
	const DBNAME = "xxxxx";
	
	public static function doConnect();
}

?>
