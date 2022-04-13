Changelog 15.09.21-14.10.21 Release 15.10.21

**Additions**
-	UnitTests for services, repositories and specifications
-	Counts on all container states and lost contact containers
	-	Endpoint for LostContact
-	GetContainerPositions: getAllContainers with minimal data
-	Boxes are bound to VehicleId, one Vehicle may have multiple boxes
-	Sorting parameters for Containers, ContainerTypes, Areas, IoT and Vehicles
	-	Area_name to GetAreaSpecification and container_type_name to GetContainerSpecification

**Fixes**
-	Issue with temperatures
-	tripRepository: Owner was set to Name
-	organizationRepository: check region unnecessary scoping and using
-	GetContainerSpecification: removed duplicated checks – might need to move some?
-	UpdateEquipment: location will no longer be overwritten by older location or future location(e.g. 2027, 2070)
