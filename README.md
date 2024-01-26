# My Bookmarks

A simple webpage that displays links to other websites. Recommended usage is via this Docker Image:

## Edit Permissions

By default, anyone can edit the bookmarks. If you want to require a passKey to edit, then specify an environmental variable named PassKey. This will prompt you to enter the passKey when you try to edit. Once successfully entered, the passKey is remembered indefinitely on that device (via localStorage). 

## What's the point?

This is more for my family's benefit rather than just myself. My family can't remember all the 192.168.x.x addresses for the various web apps on my home's LAN. By having a unified page for all the home's network stuff, they'll actually be able to find the web app on their own. I benefit from this too, since my browser bookmarks are on my main desktop computer, and I don't sync them to other devices. 

I have pi-hole, which has local DNS, but if I extensively used that it'd be hard to remember all arbitrary domain names ("camera1.com", "camera2.com", etc.). Instead, my setup just has a single local dns entry of "b.com" that points to this webpage. 

