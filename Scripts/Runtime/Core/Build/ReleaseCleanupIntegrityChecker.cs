// The lines below intentionally put here to fail the release builds.
// Release pipeline should delete this file as a build preprocess operation.
// If not, that means something went wrong in the build process and
// PlatformDependentFileRemoval failed to do it's job properly.

#if Release
ThisClassShouldBeRemovedByNow
#endif
