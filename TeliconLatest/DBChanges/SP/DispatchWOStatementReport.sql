USE `teliconlatestdb`;
DROP procedure IF EXISTS `DispatchWOStatementReport`;

USE `teliconlatestdb`;
DROP procedure IF EXISTS `teliconlatestdb`.`DispatchWOStatementReport`;
;

DELIMITER $$
USE `teliconlatestdb`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `DispatchWOStatementReport`(in fromdate date,in todate date)
BEGIN

set @row_num = 0;

SELECT @row_num := @row_num + 1 as RN,Wo_ref,Wo_title,Dispatchdt,SpliceDocs,Name,ConID 
FROM (
select distinct wo.Wo_ref, Wo_title, wo.Dispatchdt, SpliceDocs, (CD.LastName) as Name, CD.ConID  
 from trn23100 as wo
 inner join adm03400 CID on wo.Workid = cid.WorkOrderId
 inner join adm03300 CD on CID.ContractorID = CD.ConID 
 where (wo.Dispatchdt >= fromdate and wo.Dispatchdt <= todate) and 
CID.CrewLead = 1
 ) A order by A.Name, A.Dispatchdt;

END$$

DELIMITER ;
;

