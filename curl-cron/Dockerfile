FROM registry.access.redhat.com/ubi9/ubi-minimal

ENV ENDPOINT localhost:5000

# Copy the script into the container at /opt
COPY api-check.sh /opt/api-check.sh
RUN chmod +x /opt/api-check.sh

# Set the default command to run the script
ENTRYPOINT /opt/api-check.sh $ENDPOINT
