<?php
//Setup the command
//Here we are going to execute the Stop server command on serverID 0
$command['Function'] = "StopServer";
$command['Args']['id'] = 0;

//Encode the command into json
$commandJSON = json_encode($command);

//Get the JSON length for sending via TCP
$stringLen = strlen($commandJSON);

//Open the TCP socket open
$fp = fsockopen("localhost", 13000, $errno, $errstr, 10);

if (!$fp) {
    echo "$errstr ($errno)<br />\n";
} else {
	//Write the message length
    fwrite($fp,mb_convert_encoding(sprintf("%09d",strlen($commandJSON)),"ASCII"));
    fwrite($fp,$commandJSON,$stringLen); //write the message
    fclose($fp); // close the TCP socket
}
?>