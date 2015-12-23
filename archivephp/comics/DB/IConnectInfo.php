<?php
//Filename: IConnectInfo.php
interface IConnectInfo
{
	const HOST ="localhost";
	const UNAME ="acoross";
	const PW ="emfdjdhwlak";
	const DBNAME = "archive2";
	
	public static function doConnect();
}

?>
