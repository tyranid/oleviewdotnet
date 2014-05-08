import OleViewDotNet

def getobj(name):
	return OleViewDotNet.ObjectCache.GetObjectByName(name)